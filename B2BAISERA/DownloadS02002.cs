using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using B2BAISERA.Log;
using B2BAISERA.Models.Providers;
using B2BAISERA.Properties;
using System.Net;
using B2BAISERA.B2BAIWsDMZ;
using System.Globalization;
using B2BAISERA.Models;
using System.Diagnostics;

namespace B2BAISERA
{
    public partial class DownloadS02002 : Form
    {
        private static string fileType = "S02002";
        private bool acknowledge;
        private string ticketNo = "";
        private string message = "";

        public DownloadS02002()
        {
            InitializeComponent();
        }

        private void DownloadS02002_Load(object sender, EventArgs e)
        {
            LogEvent logEvent = new LogEvent();
            #region --- tidak dipakai ---
            //LoginAuthentication();
            //if (acknowledge == false || ticketNo == string.Empty)
            //{
            //    //close
            //}
            //else Download();

            //logEvent.WriteDBLog("", "DownloadS02002_Load", false, "", "Download Finish.", fileType, "SERA");
            //Process.Start("taskkill.exe", "/f /im B2BAISERA_S02002.exe");
            #endregion
            //update by fhi 03.06.2014 : add try catch
            try
            {
                LoginAuthentication();
                if (acknowledge == false || ticketNo == string.Empty)
                {
                    //close
                }
                else Download();
                logEvent.WriteDBLog("", "DownloadS02002_Load", false, "", "Download Finish.", fileType, "SERA");
                Process.Start("taskkill.exe", "/f /im B2BAISERA_S02002.exe");
            }
            catch (Exception ex)
            {
                //log file error
                logEvent.WriteDBLog("", "DownloadS02002_Load", false, "", ex.Message, fileType, "SERA");
                Process.Start("taskkill.exe", "/f /im B2BAISERA_S02002.exe");
            }
            //end
        }

        private void LoginAuthentication()
        {
            LogEvent logEvent = new LogEvent();
            TransactionProvider transactionProvider = new TransactionProvider();
            try
            {
                using (wsB2B.B2BAIWebServiceDMZ wsB2B = new wsB2B.B2BAIWebServiceDMZ())
                {
                    var User = transactionProvider.GetUser("SERA", "SERA", "B2BAITAG");
                    if (User != null)
                    {
                        var loginReq = new wsB2B.LoginRequest();
                        loginReq.UserName = User.UserCode;
                        loginReq.Password = User.PassCode;
                        loginReq.ClientTag = User.ClientTag;

                        WebProxy myProxy = new WebProxy(Resources.WebProxyAddress, true);
                        myProxy.Credentials = new NetworkCredential(Resources.NetworkCredentialUserName, Resources.NetworkCredentialPassword, Resources.NetworkCredentialProxy);

                        //WebProxy myProxy = new WebProxy();
                        //////myProxy.Credentials = new NetworkCredential(Resources.NetworkCredentialUserName, Resources.NetworkCredentialPassword, Resources.NetworkCredentialProxy);
                        //////myProxy.Credentials = new NetworkCredential("backup", "serasibackup", "trac.astra.co.id");
                        //////myProxy.Credentials = new NetworkCredential("rika009692", "mickey1988", "trac.astra.co.id");
                        //myProxy.Credentials = new NetworkCredential("genrpt", "serasera", "trac.astra.co.id");
                        wsB2B.Proxy = myProxy;

                        var wsResult = wsB2B.LoginAuthentication(loginReq);
                        acknowledge = wsResult.Acknowledge;
                        ticketNo = wsResult.TicketNo;
                        message = wsResult.Message;
                    }

                    LblResult.Text = "Service Result = ";
                    LblAcknowledge.Text = "Acknowledge : " + acknowledge;
                    LblTicketNo.Text = "TicketNo : " + ticketNo;
                    LblMessage.Text = "Message :" + message;

                    //logevent login succeded
                    logEvent.WriteDBLog("B2BAIWebServiceDMZ", "LoginAuthentication", acknowledge, ticketNo, message, fileType, "SERA");
                }
            }
            catch (Exception ex)
            {
                LblResult.Text = ex.Message;
                LblAcknowledge.Text = "";
                LblTicketNo.Text = "";
                LblMessage.Text = "";

                //logevent login failed
                logEvent.WriteDBLog("B2BAIWebServiceDMZ", "LoginAuthentication", acknowledge, ticketNo, "webservice message : " + message + ". exception message : " + ex.Message, fileType, "SERA");
                Process.Start("taskkill.exe", "/f /im B2BAISERA_S02002.exe");
            }
        }

        private void Download()
        {
            LogEvent logEvent = new LogEvent();
            try
            {
                using (wsB2B.B2BAIWebServiceDMZ wsB2B = new wsB2B.B2BAIWebServiceDMZ())
                {
                    //Sample Data Download Request
                    var downloadRequest = SampleDataDownloadRequestNew();

                    //1. DOWNLOAD FROM WS
                    //hit web service
                    WebProxy myProxy = new WebProxy(Resources.WebProxyAddress, true);
                    myProxy.Credentials = new NetworkCredential(Resources.NetworkCredentialUserName, Resources.NetworkCredentialPassword, Resources.NetworkCredentialProxy);

                    //WebProxy myProxy = new WebProxy();
                    //////myProxy.Credentials = new NetworkCredential(Resources.NetworkCredentialUserName, Resources.NetworkCredentialPassword, Resources.NetworkCredentialProxy);
                    //////myProxy.Credentials = new NetworkCredential("backup", "serasibackup", "trac.astra.co.id");
                    //////myProxy.Credentials = new NetworkCredential("rika009692", "mickey1988", "trac.astra.co.id");
                    //myProxy.Credentials = new NetworkCredential("genrpt", "serasera", "trac.astra.co.id");
                    wsB2B.Proxy = myProxy;

                    var wsResult = wsB2B.DownloadDocument(downloadRequest);
                    acknowledge = wsResult.Acknowledge;
                    ticketNo = wsResult.TicketNo;
                    message = wsResult.Message;
                    //end hit web service

                    if (wsResult.transactionData == null)
                    {
                        downloadRequest = SampleDataDownloadRequestInProgress();
                        wsResult = wsB2B.DownloadDocument(downloadRequest);
                        acknowledge = wsResult.Acknowledge;
                        ticketNo = wsResult.TicketNo;
                        message = wsResult.Message;
                    }
                    List<B2BAISERA.Models.TransactionDataViewModel> listTransactionDataModel = new List<B2BAISERA.Models.TransactionDataViewModel>();
                    if (wsResult.transactionData != null)
                    {
                        wsB2B.TransactionData[] transactionData = wsResult.transactionData;

                        for (int i = 0; i < transactionData.Length; i++)
                        {
                            B2BAISERA.Models.TransactionDataViewModel transactionDataModel = new B2BAISERA.Models.TransactionDataViewModel();
                            transactionDataModel.AIID = transactionData[i].ID;
                            transactionDataModel.TransGUID = transactionData[i].TransGUID;
                            transactionDataModel.DocumentNumber = transactionData[i].DocumentNumber;
                            transactionDataModel.FileType = transactionData[i].FileType;
                            transactionDataModel.IPAddress = transactionData[i].IPAddress;
                            transactionDataModel.DestinationUser = transactionData[i].DestinationUser;
                            transactionDataModel.Key1 = transactionData[i].Key1;
                            transactionDataModel.Key2 = transactionData[i].Key2;
                            transactionDataModel.Key3 = transactionData[i].Key3;
                            transactionDataModel.DataLength = transactionData[i].DataLength;
                            if (transactionData[i].Data.Count() > 0)
                            {
                                List<string> arrData1 = new List<string>();
                                for (int j = 0; j < transactionData[i].Data.Count(); j++)
                                {
                                    arrData1.Add(transactionData[i].Data[j]);
                                }
                                transactionDataModel.Data = arrData1.ToArray();
                            }
                            listTransactionDataModel.Add(transactionDataModel);
                        }
                    }
                    logEvent.WriteDBLog("B2BAIWebServiceDMZ", "DownloadDocumentS02002", acknowledge, ticketNo, message, fileType, "SERA");
                    LblResult.Text = "Service Result = ";
                    LblAcknowledge.Text = "Acknowledge : " + acknowledge;
                    LblTicketNo.Text = "TicketNo : " + ticketNo;
                    LblMessage.Text = "Message :" + message;

                    TransactionProvider transactionProvider = new TransactionProvider();
                    int updateTransaction = 0;
                    int insertTransaction = 0;

                    if (listTransactionDataModel.Count > 0)
                    {
                        //2. UPDATE DB EPROC FROM listTransactionDataModel 
                        updateTransaction = transactionProvider.UpdateTransactionS02002(listTransactionDataModel);

                        //3. SUCCEDED OR FAILED, CALL METHOD UPDATEDOCUMENTSTATUS
                        if (updateTransaction == 1)
                        {
                            //4. INSERT INTO CUSTOM_TRANSACTION + CUSTOM_TRANSACTIONDATA + CUSTOM_TRANSACTIONDATADETAIL + CUSTOM_S02002 
                            insertTransaction = transactionProvider.InsertLogTransactionDownloadS02002(acknowledge, ticketNo, message, "Succeeded", listTransactionDataModel);
                            if (insertTransaction == 1)
                            {
                                var updateStatusRequest = SucceededDataUpdateStatusRequest(listTransactionDataModel);
                                var wsResultUpdateDocStatus = wsB2B.UpdateDocumentStatus(updateStatusRequest);
                                acknowledge = wsResultUpdateDocStatus.Acknowledge;
                                ticketNo = wsResultUpdateDocStatus.TicketNo;
                                message = wsResultUpdateDocStatus.Message;
                            }
                        }
                        else
                        {
                            //4. INSERT INTO CUSTOM_TRANSACTION + CUSTOM_TRANSACTIONDATA + CUSTOM_TRANSACTIONDATADETAIL + CUSTOM_S02002 
                            insertTransaction = transactionProvider.InsertLogTransactionDownloadS02002(acknowledge, ticketNo, message, "Failed", listTransactionDataModel);
                            if (insertTransaction == 1)
                            {
                                //var updateStatusRequest = FailedDataUpdateStatusRequest(listTransactionDataModel);
                                //var wsResultUpdateDocStatus = wsB2B.UpdateDocumentStatus(updateStatusRequest);
                                //acknowledge = wsResultUpdateDocStatus.Acknowledge;
                                //ticketNo = wsResultUpdateDocStatus.TicketNo;
                                //message = wsResultUpdateDocStatus.Message;
                                ////message += ", but Update Data Failed.";  
                            }
                        }
                        logEvent.WriteDBLog("B2BAIWebServiceDMZ", "UpdateDocumentStatusS02002", acknowledge, ticketNo, message, fileType, "SERA");

                        LblResult.Text = "Service Result = ";
                        LblAcknowledge.Text = "Acknowledge : " + acknowledge;
                        LblTicketNo.Text = "TicketNo : " + ticketNo;
                        LblMessage.Text = "Message :" + message;
                    }
                }
            }
            catch (Exception ex)
            {
                LblResult.Text = ex.Message;

                logEvent.WriteDBLog("B2BAIWebServiceDMZ", "DownloadDocumentS02002", acknowledge, ticketNo, ex.Message, fileType, "SERA");
            }
        }

        private wsB2B.DownloadRequest SampleDataDownloadRequestNew()
        {
            wsB2B.DownloadRequest downloadRequest = new wsB2B.DownloadRequest();
            TransactionProvider transactionProvider = new TransactionProvider();
            var lastTicketNo = transactionProvider.GetLastTicketNo(fileType);
            downloadRequest.TicketNo = lastTicketNo;
            downloadRequest.ClientTag = Resources.ClientTag;
            downloadRequest.FileType = "S02002";
            downloadRequest.SourceUser = "AI";
            downloadRequest.Status = "New";
            downloadRequest.TransDateFrom = DateTime.Now.AddDays(-180);//Convert.ToDateTime("2013-10-29T00:00:00", new CultureInfo("id-ID"));
            downloadRequest.TransDateTo = DateTime.Now;//Convert.ToDateTime("2013-10-30T00:00:00", new CultureInfo("id-ID"));
            //  downloadRequest.TransDateFrom = Convert.ToDateTime("2015-07-30T00:00:00", new CultureInfo("id-ID"));
            //  downloadRequest.TransDateTo = Convert.ToDateTime("2015-07-31T00:00:00", new CultureInfo("id-ID"));
            //downloadRequest.TransDateFrom = Convert.ToDateTime("2015-07-27T00:00:00", new CultureInfo("id-ID"));
            //downloadRequest.TransDateTo = Convert.ToDateTime("2015-07-29T00:00:00", new CultureInfo("id-ID"));
            downloadRequest.Key1 = ""; //sap:T212 //pss:T153
            downloadRequest.Key2 = ""; //"SAP"; //"PSS";
            downloadRequest.Key3 = ""; //"4111021716"; // "4111031630"; //"4111024893";//"4111028442"; //"4111032511"; //documentnumber

            return downloadRequest;
        }

        private wsB2B.DownloadRequest SampleDataDownloadRequestInProgress()
        {
            wsB2B.DownloadRequest downloadRequest = new wsB2B.DownloadRequest();
            TransactionProvider transactionProvider = new TransactionProvider();
            var lastTicketNo = transactionProvider.GetLastTicketNo(fileType);
            downloadRequest.TicketNo = lastTicketNo;
            downloadRequest.ClientTag = Resources.ClientTag;
            downloadRequest.FileType = "S02002";
            downloadRequest.SourceUser = "AI";
            downloadRequest.Status = "InProgress";
            downloadRequest.TransDateFrom = DateTime.Now.AddDays(-180);
            downloadRequest.TransDateTo = DateTime.Now;
            // downloadRequest.TransDateFrom = Convert.ToDateTime("2015-07-30T00:00:00", new CultureInfo("id-ID"));
            // downloadRequest.TransDateTo = Convert.ToDateTime("2015-07-31T00:00:00", new CultureInfo("id-ID"));
            downloadRequest.Key1 = ""; //sap:T212 //pss:T153
            downloadRequest.Key2 = ""; //"SAP"; //"PSS";
            downloadRequest.Key3 = ""; //"4111021716"; // "4111031630"; //"4111024893";//"4111028442"; //"4111032511"; //documentnumber

            return downloadRequest;
        }

        private wsB2B.UpdateStatusRequest SucceededDataUpdateStatusRequest(List<TransactionDataViewModel> listTransactionDataModel)
        {
            List<B2BAISERA.wsB2B.TransactionDataID> listTransactionDataID = new List<wsB2B.TransactionDataID>();
            for (int i = 0; i < listTransactionDataModel.Count; i++)
            {
                B2BAISERA.wsB2B.TransactionDataID transactionDataID = new B2BAISERA.wsB2B.TransactionDataID();
                transactionDataID.TransGUID = listTransactionDataModel[i].TransGUID;
                transactionDataID.DocumentNumber = listTransactionDataModel[i].DocumentNumber;
                transactionDataID.Key1 = listTransactionDataModel[i].Key1; //"T153"; 
                transactionDataID.Key2 = listTransactionDataModel[i].Key2;
                transactionDataID.Key3 = listTransactionDataModel[i].Key3;
                transactionDataID.TransStatus = "Succeeded";
                transactionDataID.LogMessage = listTransactionDataModel[i].DocumentNumber + " SUKSES di-proses";
                listTransactionDataID.Add(transactionDataID);
            }

            wsB2B.UpdateStatusRequest updateStatusRequest = new wsB2B.UpdateStatusRequest();
            TransactionProvider transactionProvider = new TransactionProvider();
            var lastTicketNo = transactionProvider.GetLastTicketNo(fileType);
            updateStatusRequest.TicketNo = lastTicketNo;
            updateStatusRequest.ClientTag = Resources.ClientTag;
            updateStatusRequest.transactionDataID = listTransactionDataID.ToArray();

            return updateStatusRequest;
        }

        private wsB2B.UpdateStatusRequest FailedDataUpdateStatusRequest(List<TransactionDataViewModel> listTransactionDataModel)
        {
            List<B2BAISERA.wsB2B.TransactionDataID> listTransactionDataID = new List<wsB2B.TransactionDataID>();
            for (int i = 0; i < listTransactionDataModel.Count; i++)
            {
                B2BAISERA.wsB2B.TransactionDataID transactionDataID = new B2BAISERA.wsB2B.TransactionDataID();
                transactionDataID.TransGUID = listTransactionDataModel[i].TransGUID;
                transactionDataID.DocumentNumber = listTransactionDataModel[i].DocumentNumber;
                transactionDataID.Key1 = listTransactionDataModel[i].Key1; //"T153";
                transactionDataID.Key2 = listTransactionDataModel[i].Key2;
                transactionDataID.Key3 = listTransactionDataModel[i].Key3;
                transactionDataID.TransStatus = "Failed";
                transactionDataID.LogMessage = listTransactionDataModel[i].DocumentNumber + " GAGAL di-proses";
                listTransactionDataID.Add(transactionDataID);
            }

            wsB2B.UpdateStatusRequest updateStatusRequest = new wsB2B.UpdateStatusRequest();
            TransactionProvider transactionProvider = new TransactionProvider();
            var lastTicketNo = transactionProvider.GetLastTicketNo(fileType);
            updateStatusRequest.TicketNo = lastTicketNo;
            updateStatusRequest.ClientTag = Resources.ClientTag;
            updateStatusRequest.transactionDataID = listTransactionDataID.ToArray();

            return updateStatusRequest;
        }
    }
}
