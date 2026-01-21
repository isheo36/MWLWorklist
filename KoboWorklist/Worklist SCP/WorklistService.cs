// Copyright (c) 2012-2025 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using FellowOakDicom;
using FellowOakDicom.Network;
using KoboWorklist.WorklistSCP.Model;
using Microsoft.Extensions.Logging;
using System.Text;

namespace KoboWorklist.WorklistSCP
{
    public class WorklistService : DicomService, IDicomServiceProvider, IDicomCEchoProvider, IDicomCFindProvider
    {
        private static readonly DicomTransferSyntax[] _acceptedTransferSyntaxes = new DicomTransferSyntax[]
        {
            DicomTransferSyntax.ExplicitVRLittleEndian,
            DicomTransferSyntax.ExplicitVRBigEndian,
            DicomTransferSyntax.ImplicitVRLittleEndian
        };

        private readonly ILogger _logger;

        public WorklistService(INetworkStream stream, Encoding fallbackEncoding, ILogger log, DicomServiceDependencies dependencies)
            : base(stream, fallbackEncoding, log, dependencies)
        {
            _logger = log;
        }

        public async Task<DicomCEchoResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
        {
            _logger.LogInformation($"Received verification request from AE {Association.CallingAE} with IP: {Association.RemoteHost}");
            return new DicomCEchoResponse(request, DicomStatus.Success);
        }

        public async IAsyncEnumerable<DicomCFindResponse> OnCFindRequestAsync(DicomCFindRequest request)
        {
            foreach ( var item in request.Dataset)
            {
                string tagName = item.Tag.DictionaryEntry.Name;
                string value = "";

                // ПРОВЕРКА: Само елементи, които не са Sequence (SQ), имат директна стрингова стойност
                if (item is DicomElement element)
                {
                    // Сега е безопасно да извикаме GetString
                    value = request.Dataset.GetString(item.Tag);
                }
                else if (item is DicomSequence sequence)
                {
                    _logger.LogInformation($"{item.Tag} {tagName} [SEQUENCE START]");

                    // Итерираме през всеки Dataset (Item) в секвенцията
                    int itemCounter = 1;
                    foreach (var seqItem in sequence.Items)
                    {
                        _logger.LogInformation($"  --- Sequence Item #{itemCounter} ---");

                        // Всеки seqItem е всъщност DicomDataset
                        foreach (var subItem in seqItem)
                        {
                            string subTagName = subItem.Tag.DictionaryEntry.Name;
                            string subValue = seqItem.GetString(subItem.Tag);
                            _logger.LogInformation($"    {subItem.Tag} {subTagName}: {subValue}");
                        }
                        itemCounter++;
                    }

                    _logger.LogInformation($"{item.Tag} {tagName} [SEQUENCE END]");
                }
                else
                {
                    value = "[Друг тип данни]";
                }

                _logger.LogInformation($"{item.Tag} | {tagName} | {item.ValueRepresentation} | Value: {value}");
            }

            foreach (var result in WorklistHandler.FilterWorklistItems(
                request.Dataset,
                WorklistServer.CurrentWorklistItems))
            {
                yield return new DicomCFindResponse(request, DicomStatus.Pending) { Dataset = result };
            }
            yield return new DicomCFindResponse(request, DicomStatus.Success);
        }

        public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
        {
            _logger.LogInformation($"Received association request from AE: {association.CallingAE} with IP: {association.RemoteHost}");

            if (WorklistServer.AETitle != association.CalledAE)
            {
                _logger.LogError($"Association rejected: unknown AE Title {association.CalledAE}");
                return SendAssociationRejectAsync(DicomRejectResult.Permanent, DicomRejectSource.ServiceUser, DicomRejectReason.CalledAENotRecognized);
            }

            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.ModalityWorklistInformationModelFind)
                {
                    pc.AcceptTransferSyntaxes(_acceptedTransferSyntaxes);
                }
                else
                {
                    _logger.LogWarning($"Unsupported abstract syntax {pc.AbstractSyntax}");
                    pc.SetResult(DicomPresentationContextResult.RejectAbstractSyntaxNotSupported);
                }
            }

            _logger.LogInformation($"Accepted association request from {association.CallingAE}");
            return SendAssociationAcceptAsync(association);
        }

        public void OnConnectionClosed(Exception exception)
        {
            _logger.LogInformation("Connection closed.");
        }

        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
            _logger.LogError($"Received abort: {source}, Reason: {reason}");
        }

        public Task OnReceiveAssociationReleaseRequestAsync()
        {
            _logger.LogInformation("Association released.");
            return SendAssociationReleaseResponseAsync();
        }
    }
}
