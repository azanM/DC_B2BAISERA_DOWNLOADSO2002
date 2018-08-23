using System;
using System.Collections.Generic;
using System.Linq;
//using B2BAISERA.Models.DataAccess;
//using B2BAISERA.Helper;
//using B2BAISERA.Logic;
using Microsoft.Practices.Unity;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Common;
using B2BAISERA.Models.EFServer;
using B2BAISERA.Helper;
using System.Data.EntityClient;
using System.Data;
using B2BAISERA.B2BAIWsDMZ;
using System.Globalization;

namespace B2BAISERA.Models.Providers
{
    public class TransactionProvider : DataAccessBase
    {
        public TransactionProvider()
            : base()
        {
        }

        public TransactionProvider(EProcEntities context)
            : base(context)
        {
        }

        #region MAIN
        //B2BAISERAEntities ctx = new B2BAISERAEntities(Repository.ConnectionStringEF);

        //public User GetUser(string userName, string password, string clientTag)
        //{
        //    var User = (from o in ctx.Users
        //                where o.UserName == userName && o.Password == password && o.ClientTag == clientTag
        //                select o).FirstOrDefault();

        //    return User;
        //}

        public CUSTOM_USER GetUser(string userName, string password, string clientTag)
        {
            var user = (from o in entities.CUSTOM_USER
                        where o.UserName == userName && o.Password == password && o.ClientTag == clientTag
                        select o).FirstOrDefault();

            return user;
        }

        public string GetLastTicketNo(string fileType)
        {
            var result = "";
            try
            {
                var query = (entities.CUSTOM_LOG
                    .Where(log => (log.Acknowledge == true) && (log.FileType == fileType))
                    .Select(log => new LogViewModel
                    {
                        ID = log.ID,
                        WebServiceName = log.WebServiceName,
                        MethodName = log.MethodName,
                        TicketNo = log.TicketNo,
                        Message = log.Message
                    })
                    ).OrderByDescending(log => log.ID).FirstOrDefault();

                result = query != null ? Convert.ToString(query.TicketNo) : "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        #endregion

        #region DOWNLOAD

        #region S02002
        public int InsertLogTransactionDownloadS02002(bool acknowledge, string ticketNo, string message, string transStatus, List<TransactionDataViewModel> listTransactionDataModel)
        {
            List<string> errorCatch2 = new List<string>();
            List<CUSTOM_DOWNLOAD_TRANSACTION> listC1 = new List<CUSTOM_DOWNLOAD_TRANSACTION>();
            List<CUSTOM_DOWNLOAD_TRANSACTIONDATA> listC2 = new List<CUSTOM_DOWNLOAD_TRANSACTIONDATA>();
            List<CUSTOM_DOWNLOAD_TRANSACTIONDATADETAIL> listC3 = new List<CUSTOM_DOWNLOAD_TRANSACTIONDATADETAIL>();
            List<CUSTOM_S02002> listC4 = new List<CUSTOM_S02002>();
            int result = 0;
            try
            {
                //insert into CUSTOM_DOWNLOAD_TRANSACTION
                CUSTOM_DOWNLOAD_TRANSACTION transaction = new CUSTOM_DOWNLOAD_TRANSACTION();
                transaction.Acknowledge = acknowledge;
                transaction.TicketNo = ticketNo;
                transaction.Message = message;
                EntityHelper.SetAuditForInsert(transaction, "SERA");
                entities.CUSTOM_DOWNLOAD_TRANSACTION.AddObject(transaction);
                listC1.Add(transaction);
                for (int i = 0; i < listTransactionDataModel.Count; i++)
                {
                    try
                    {

                        //insert into CUSTOM_DOWNLOAD_TRANSACTIONDATA
                        CUSTOM_DOWNLOAD_TRANSACTIONDATA transactionData = new CUSTOM_DOWNLOAD_TRANSACTIONDATA();
                        transactionData.CUSTOM_DOWNLOAD_TRANSACTION = transaction;
                        transactionData.AIID = listTransactionDataModel[i].AIID;
                        transactionData.TransGUID = listTransactionDataModel[i].TransGUID;
                        transactionData.DocumentNumber = listTransactionDataModel[i].DocumentNumber;
                        transactionData.FileType = listTransactionDataModel[i].FileType;
                        transactionData.IPAddress = listTransactionDataModel[i].IPAddress;
                        transactionData.DestinationUser = listTransactionDataModel[i].DestinationUser;
                        transactionData.Key1 = listTransactionDataModel[i].Key1;
                        transactionData.Key2 = listTransactionDataModel[i].Key2;
                        transactionData.Key3 = listTransactionDataModel[i].Key3;
                        transactionData.DataLength = listTransactionDataModel[i].DataLength;
                        transactionData.TransStatus = transStatus;
                        EntityHelper.SetAuditForInsert(transactionData, "SERA");
                        entities.CUSTOM_DOWNLOAD_TRANSACTIONDATA.AddObject(transactionData);
                        listC2.Add(transactionData);

                        S02002ViewModel modelHSSO2002 = new S02002ViewModel();
                        S02002ViewModel modelISSO2002 = new S02002ViewModel();
                        for (int j = 0; j < listTransactionDataModel[i].Data.Length; j++)
                        {
                            //switch (j)
                            //{
                            //    case 0:
                            //        modelHSSO2002 = SplitStringHSS02002(listTransactionDataModel[i].Data[j]);
                            //        break;
                            //    case 1:
                            //        modelISSO2002 = SplitStringISS02002(listTransactionDataModel[i].Data[j]);
                            //        break;
                            //    default:
                            //        break;
                            //}
                            if (listTransactionDataModel[i].Data[j].Split('|')[0].Trim().Contains("HS"))
                            {
                                modelHSSO2002 = SplitStringHSS02002(listTransactionDataModel[i].Data[j]);
                            }
                            else if (listTransactionDataModel[i].Data[j].Split('|')[0].Trim().Contains("IS"))
                            {
                                modelISSO2002 = SplitStringISS02002(listTransactionDataModel[i].Data[j]);
                            }
                            //insert into CUSTOM_DOWNLOAD_TRANSACTIONDATADETAIL
                            CUSTOM_DOWNLOAD_TRANSACTIONDATADETAIL transactionDataDetail = new CUSTOM_DOWNLOAD_TRANSACTIONDATADETAIL();
                            transactionDataDetail.CUSTOM_DOWNLOAD_TRANSACTIONDATA = transactionData;
                            transactionDataDetail.Data = listTransactionDataModel[i].Data[j];
                            entities.CUSTOM_DOWNLOAD_TRANSACTIONDATADETAIL.AddObject(transactionDataDetail);
                            listC3.Add(transactionDataDetail);
                        }

                        //insert into CUSTOM_S02002 FOR HS
                        if (!string.IsNullOrEmpty(modelHSSO2002.PONumber))
                        {
                            CUSTOM_S02002 s02002 = new CUSTOM_S02002();
                            s02002.CUSTOM_DOWNLOAD_TRANSACTIONDATA = transactionData;
                            s02002.PONumber = modelHSSO2002.PONumber;
                            s02002.VersionPOSERA = modelHSSO2002.VersionPOSERA;
                            s02002.DataVersion = modelHSSO2002.DataVersion;
                            s02002.StatusPOSERA = modelHSSO2002.StatusPOSERA;
                            s02002.RejectRevisedPOSERA = modelHSSO2002.RejectRevisedPOSERA;
                            s02002.DocumentNo = modelHSSO2002.DocumentNo;
                            s02002.AIMaterialNumber = modelHSSO2002.AIMaterialNumber;
                            s02002.SERAMaterialNumber = modelHSSO2002.SERAMaterialNumber;
                            s02002.SERAMaterialDescription = modelHSSO2002.SERAMaterialDescription;
                            s02002.AIColor = modelHSSO2002.AIColor;
                            s02002.SERAColor = modelHSSO2002.SERAColor;
                            s02002.QuotationNo = modelHSSO2002.QuotationNo;
                            if (modelISSO2002 != null)
                            {
                                s02002.SalesOrderNo = modelISSO2002.SalesOrderNo;
                                s02002.SalesOrderStatus = modelISSO2002.SalesOrderStatus;
                                s02002.DPPByVendor = modelISSO2002.DPPByVendor;
                                s02002.PPNByVendor = modelISSO2002.PPNByVendor;
                                s02002.BBNPriceByVendor = modelISSO2002.BBNPriceByVendor;
                                s02002.Currency = modelISSO2002.Currency;
                                s02002.ChassisNumberByVendor = modelISSO2002.ChassisNumberByVendor;
                                s02002.MachineNumberByVendor = modelISSO2002.MachineNumberByVendor;
                                s02002.CBUCKD = modelISSO2002.CBUCKD;
                                s02002.Year = modelISSO2002.Year;
                                s02002.FactureDONumber = modelISSO2002.FactureDONumber;
                                s02002.BillingStatus = modelISSO2002.BillingStatus;
                                s02002.FactureDODate = modelISSO2002.FactureDODate;
                                s02002.NoFakturKendaraan = modelISSO2002.NoFakturKendaraan;
                                s02002.TanggalFakturKendaraan = modelISSO2002.TanggalFakturKendaraan;
                                s02002.CancellationReason = modelISSO2002.CancellationReason;
                                s02002.ActualDateDeliveryUnit = modelISSO2002.ActualDateDeliveryUnit;
                                s02002.BSTKBNo = modelISSO2002.BSTKBNo;
                                s02002.LicensePlateByVendor = modelISSO2002.LicensePlateByVendor;
                                s02002.STNKDateByVendor = modelISSO2002.STNKDateByVendor;
                                s02002.RevisiSTNK = modelISSO2002.RevisiSTNK;
                                s02002.NoSertifikat = modelISSO2002.NoSertifikat;
                                s02002.TglSertifikat = modelISSO2002.TglSertifikat;
                                s02002.NoFormulirA = modelISSO2002.NoFormulirA;
                                s02002.TglFormulirA = modelISSO2002.TglFormulirA;
                                s02002.NoSertifikatRegUjiTipe = modelISSO2002.NoSertifikatRegUjiTipe;
                                s02002.ActualDeliveryBPKBDate = modelISSO2002.ActualDeliveryBPKBDate;
                                s02002.NamaPenerima = modelISSO2002.NamaPenerima;
                                s02002.AlamatPenerima = modelISSO2002.AlamatPenerima;
                                s02002.BPKBNumber = modelISSO2002.BPKBNumber;
                                s02002.RemarksBPKB = modelISSO2002.RemarksBPKB;
                                s02002.RevisiBPKB = modelISSO2002.RevisiBPKB;
                            }
                            //start add by fhi 18.06.2014 : set owner
                            s02002.dibuatOleh = "system";
                            s02002.dibuatTanggal = DateTime.Now;
                            s02002.diubahOleh = "system";
                            s02002.diubahTanggal = DateTime.Now;
                            //end
                            entities.CUSTOM_S02002.AddObject(s02002);
                            listC4.Add(s02002);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCatch2.Add(i + " | " + listTransactionDataModel[i].DocumentNumber + " | " + ex.Message);

                    }
                }
                entities.CommandTimeout = 50000;
                entities.SaveChanges();
                result = 1;
            }
            catch (Exception ex)
            {
                result = 0;
                throw ex;
            }
            return result;
        }

        private S02002ViewModel SplitStringHSS02002(string stringHS)
        {
            S02002ViewModel model = new S02002ViewModel();
            try
            {
                //TODO 21-08-2013:VALIDASI SUBSTRING
                string[] words = stringHS.Split('|');
                for (int i = 1; i < words.Length; i++)
                {
                    if (!string.IsNullOrEmpty(words[i]))
                    {
                        decimal decimalValue;
                        var date = words[i].Trim();
                        switch (i)
                        {
                            case 1:
                                model.PONumber = words[i].Length > 50 ? words[i].Substring(0, 50).Trim() : words[i].Trim();
                                break;
                            case 2:
                                if (Decimal.TryParse(words[i], out decimalValue))
                                {
                                    model.VersionPOSERA = Convert.ToDecimal(words[i]);
                                }
                                break;
                            case 3:
                                if (Decimal.TryParse(words[i], out decimalValue))
                                {
                                    model.DataVersion = Convert.ToDecimal(words[i]);
                                }
                                break;
                            case 4:
                                model.StatusPOSERA = words[i];
                                break;
                            case 5:
                                model.RejectRevisedPOSERA = words[i];
                                break;
                            case 6:
                                model.DocumentNo = words[i];
                                break;
                            case 7:
                                model.AIMaterialNumber = words[i];
                                break;
                            case 8:
                                model.SERAMaterialNumber = words[i];
                                break;
                            case 9:
                                model.SERAMaterialDescription = words[i];
                                break;
                            case 10:
                                model.AIColor = words[i];
                                break;
                            case 11:
                                model.SERAColor = words[i];
                                break;
                            case 12:
                                var lastWord = words[i].Remove(words[i].IndexOf('}')).Trim();
                                model.QuotationNo = lastWord;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return model;
        }

        private S02002ViewModel SplitStringISS02002(string stringHS)
        {
            S02002ViewModel model = new S02002ViewModel();
            try
            {
                //TODO 21-08-2013:VALIDASI SUBSTRING
                string[] words = stringHS.Split('|');
                for (int i = 1; i < words.Length; i++)
                {
                    if (!string.IsNullOrEmpty(words[i]))
                    {
                        double doubleValue;
                        int intValue;
                        //DateTime datetimeValue;
                        var date = words[i].Trim();
                        switch (i)
                        {
                            case 1:
                                model.SalesOrderNo = words[i].Length > 15 ? words[i].Substring(0, 15).Trim() : words[i].Trim();
                                break;
                            case 2:
                                model.SalesOrderStatus = words[i] == "1" ? true : false;
                                break;
                            case 3:
                                if (Double.TryParse(words[i], out doubleValue))
                                {
                                    model.DPPByVendor = Convert.ToDouble(words[i]);
                                }
                                break;
                            case 4:
                                if (Double.TryParse(words[i], out doubleValue))
                                {
                                    model.PPNByVendor = Convert.ToDouble(words[i]);
                                }
                                break;
                            case 5:
                                if (Double.TryParse(words[i], out doubleValue))
                                {
                                    model.BBNPriceByVendor = Convert.ToDouble(words[i]);
                                }
                                break;
                            case 6:
                                model.Currency = words[i];
                                break;
                            case 7:
                                model.ChassisNumberByVendor = words[i];
                                break;
                            case 8:
                                model.MachineNumberByVendor = words[i];
                                break;
                            case 9:
                                model.CBUCKD = words[i];
                                break;
                            case 10:
                                if (Int32.TryParse(words[i], out intValue))
                                {
                                    model.Year = Convert.ToInt32(words[i]);
                                }
                                break;
                            case 11:
                                model.FactureDONumber = words[i];
                                break;
                            case 12:
                                model.BillingStatus = words[i] == "1" ? true : false;
                                break;
                            case 13:
                                //if (words[i].Substring(0, 2) == "20")
                                //{
                                //    if (DateTime.TryParse(words[i], out datetimeValue))
                                //    {
                                //        model.FactureDODate = Convert.ToDateTime(words[i]);
                                //    }
                                //}
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.FactureDODate = dateFormat;
                                    //}
                                }
                                break;
                            case 14:
                                model.NoFakturKendaraan = words[i];
                                break;
                            case 15:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.TanggalFakturKendaraan = dateFormat;
                                    //}
                                }
                                break;
                            case 16:
                                model.CancellationReason = words[i];
                                break;
                            case 17:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.ActualDateDeliveryUnit = dateFormat;
                                    //}
                                }
                                break;
                            case 18:
                                model.BSTKBNo = words[i];
                                break;
                            case 19:
                                model.LicensePlateByVendor = words[i];
                                break;
                            case 20:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.STNKDateByVendor = dateFormat;
                                    //}
                                }
                                break;
                            case 21:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.RevisiSTNK = dateFormat;
                                    //}
                                }
                                break;
                            case 22:
                                model.NoSertifikat = words[i];
                                break;
                            case 23:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.TglSertifikat = dateFormat;
                                    //}
                                }
                                break;
                            case 24:
                                model.NoFormulirA = words[i];
                                break;
                            case 25:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.TglFormulirA = dateFormat;
                                    //}
                                }
                                break;
                            case 26:
                                model.NoSertifikat = words[i];
                                break;
                            case 27:
                                if (date.Length == 8)
                                {
                                    //if (date.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.ActualDeliveryBPKBDate = dateFormat;
                                    //}
                                }
                                break;
                            case 28:
                                model.NamaPenerima = words[i];
                                break;
                            case 29:
                                model.AlamatPenerima = words[i];
                                break;
                            case 30:
                                model.BPKBNumber = words[i];
                                break;
                            case 31:
                                model.RemarksBPKB = words[i];
                                break;
                            case 32:
                                var lastDate = words[i].Remove(words[i].IndexOf('}')).Trim();
                                if (lastDate.Length == 8)
                                {
                                    //if (lastDate.Substring(0, 2) == "20")
                                    //{
                                    var dateFormat = Convert.ToDateTime(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2), new CultureInfo("id-ID"));
                                    model.RevisiBPKB = dateFormat;
                                    //}
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return model;
        }

        public int UpdateTransactionS02002(List<TransactionDataViewModel> listTransactionDataModel)
        {
            int result = 0;
            List<string> errorCatch = new List<string>();
            try
            {
                for (int i = 0; i < listTransactionDataModel.Count; i++)
                {
                    string poNumber = listTransactionDataModel[i].Data[0].Split('|')[1].Trim();
                    if (!string.IsNullOrEmpty(poNumber)) {
                        CUSTOMPO customPO = entities.CUSTOMPOes.SingleOrDefault(x => x.PONUMBER == poNumber);
                        CUSTOMGR customGR = entities.CUSTOMGRs.SingleOrDefault(x => x.PONUMBER == poNumber);
                        CUSTOMBPKB customBPKB = entities.CUSTOMBPKBs.SingleOrDefault(x => x.PONUMBER == poNumber);

                        for (int j = 0; j < listTransactionDataModel[i].Data.Length; j++)
                        {
                            try
                            {
                                S02002ViewModel modelSO2002 = new S02002ViewModel();
                                //switch (j)
                                //{
                                //    case 0:
                                //        S02002ViewModel modelHSSO2002 = SplitStringHSS02002(listTransactionDataModel[i].Data[j]);
                                //        break;
                                //    case 1:
                                //        S02002ViewModel modelISSO2002 = SplitStringISS02002(listTransactionDataModel[i].Data[j]);
                                //        break;
                                //    default:
                                //        break;
                                //}
                                if (listTransactionDataModel[i].Data[j].Split('|')[0].Trim().Contains("HS"))
                                {
                                    modelSO2002 = SplitStringHSS02002(listTransactionDataModel[i].Data[j]);
                                }
                                else if (listTransactionDataModel[i].Data[j].Split('|')[0].Trim().Contains("IS"))
                                {
                                    modelSO2002 = SplitStringISS02002(listTransactionDataModel[i].Data[j]);
                                }

                                //HS-IS update
                                if (listTransactionDataModel[i].Data[j].Split('|')[0].Trim().Contains("HS"))
                                    result = UpdateS02002HS(poNumber, "HS", modelSO2002);
                                else if (listTransactionDataModel[i].Data[j].Split('|')[0].Trim().Contains("IS"))
                                    if (customPO != null && customGR != null && customBPKB != null)
                                        result = UpdateS02002IS(poNumber, "IS", modelSO2002);
                                //CustomPO Update
                                if (customGR.ACTUALRECEIVEDUNIT.HasValue && modelSO2002.ActualDateDeliveryUnit.HasValue && customPO.ACTUALDATEDELIVEREDUNIT.HasValue)
                                    UpdateCustomPOStatusPOId(poNumber, "5");
                            }
                            catch (Exception ex)
                            {
                                errorCatch.Add(poNumber + " | " + ex.Message);
                                //something
                            }
                        }
                    }
                    } 
                   
            }
            catch (Exception ex)
            {
                return result = 0;
            }

            return result;
        }

        public int UpdateS02002HS(string poNumber, string HSorIS, S02002ViewModel model)
        {
            int result;
            try
            {
                EntityCommand cmd = new EntityCommand("EProcEntities.sp_UpdateS02002_HS", entityConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("PONUMBER", DbType.String).Value = poNumber;
                //CUSTOM_VENDOR_TRANSATION
                cmd.Parameters.Add("VERSIONPOSERA", DbType.Int32).Value = Convert.ToInt32(model.VersionPOSERA);
                cmd.Parameters.Add("DATAVERSION", DbType.Int32).Value = Convert.ToInt32(model.DataVersion);
                cmd.Parameters.Add("STATUSPOSERA", DbType.String).Value = model.StatusPOSERA;
                cmd.Parameters.Add("REJECTREVISEDPOSERA", DbType.String).Value = model.RejectRevisedPOSERA;
                cmd.Parameters.Add("DOCUMENTNO", DbType.String).Value = model.DocumentNo;
                cmd.Parameters.Add("AIMATERIALNUMBER", DbType.String).Value = model.AIMaterialNumber;
                cmd.Parameters.Add("SERAMATERIALNUMBER", DbType.String).Value = model.SERAMaterialNumber;
                cmd.Parameters.Add("SERAMATERIALDESCRIPTION", DbType.String).Value = model.SERAMaterialDescription;
                cmd.Parameters.Add("AICOLOR", DbType.String).Value = model.AIColor;
                cmd.Parameters.Add("SERACOLOR", DbType.String).Value = model.SERAColor;
                cmd.Parameters.Add("QUOTATIONNO", DbType.String).Value = model.QuotationNo;
                //cmd.Parameters.Add("HSORIS", DbType.String).Value = HSorIS;
                OpenConnection();
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                result = 0;
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
            return result;
        }

        public int UpdateS02002IS(string poNumber, string HSorIS, S02002ViewModel model)
        {
            int result;
            try
            {
                EntityCommand cmd = new EntityCommand("EProcEntities.sp_UpdateS02002_IS", entityConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("PONUMBER", DbType.String).Value = poNumber;
                cmd.Parameters.Add("HARGADPP_INPUT", DbType.Double).Value = model.DPPByVendor;
                cmd.Parameters.Add("HARGAPPNUNIT_INPUT", DbType.Double).Value = model.PPNByVendor;
                cmd.Parameters.Add("HARGABBN_INPUT", DbType.Double).Value = model.BBNPriceByVendor;
                // cmd.Parameters.Add("CBUCKD", DbType.String).Value = model.CBUCKD;// tidak ada di table
                cmd.Parameters.Add("DONUMBER", DbType.String).Value = model.FactureDONumber;
                cmd.Parameters.Add("DODATE", DbType.DateTime).Value = model.FactureDODate;
                cmd.Parameters.Add("ACTUALDATEDELIVEREDUNIT", DbType.DateTime).Value = model.ActualDateDeliveryUnit;
                cmd.Parameters.Add("NOCHASIS_INPUT", DbType.String).Value = model.ChassisNumberByVendor;
                cmd.Parameters.Add("NOENGINE_INPUT", DbType.String).Value = model.MachineNumberByVendor;
                cmd.Parameters.Add("NOFAKTUR", DbType.String).Value = model.NoFakturKendaraan;
                cmd.Parameters.Add("TGLFAKTUR", DbType.DateTime).Value = model.TanggalFakturKendaraan;
                cmd.Parameters.Add("NOPOLISI_INPUT", DbType.String).Value = model.LicensePlateByVendor;
                cmd.Parameters.Add("TGLSTNK_INPUT", DbType.DateTime).Value = model.STNKDateByVendor;
                cmd.Parameters.Add("NOSERTIFIKAT", DbType.String).Value = model.NoSertifikat;
                cmd.Parameters.Add("TGLSERTIFIKAT", DbType.DateTime).Value = model.TglSertifikat;
                cmd.Parameters.Add("NOFORMULIRA", DbType.String).Value = model.NoFormulirA;
                cmd.Parameters.Add("TGLFORMULIRA", DbType.DateTime).Value = model.TglFormulirA;
                cmd.Parameters.Add("NOSERTIFIKATREGUJITIPE", DbType.String).Value = model.NoSertifikatRegUjiTipe;
                cmd.Parameters.Add("DATEDELIVERYTOBRANCHORVENDOR", DbType.DateTime).Value = model.ActualDeliveryBPKBDate;
                cmd.Parameters.Add("NOBPKB", DbType.String).Value = model.BPKBNumber;
                cmd.Parameters.Add("KETBPKB", DbType.String).Value = model.RemarksBPKB;
                cmd.Parameters.Add("REVISEDATE", DbType.DateTime).Value = model.RevisiBPKB;
                ////CUSTOM_VENDOR_TRANSATION
                //cmd.Parameters.Add("VERSIONPOSERA", DbType.Int32).Value = model.VersionPOSERA;
                //cmd.Parameters.Add("DATAVERSION", DbType.Int32).Value = model.DataVersion;
                //cmd.Parameters.Add("STATUSPOSERA", DbType.String).Value = model.StatusPOSERA;
                //cmd.Parameters.Add("REJECTREVISEDPOSERA", DbType.String).Value = model.RejectRevisedPOSERA;
                //cmd.Parameters.Add("DOCUMENTNO", DbType.String).Value = model.DocumentNo;
                //cmd.Parameters.Add("AIMATERIALNUMBER", DbType.String).Value = model.AIMaterialNumber;
                //cmd.Parameters.Add("SERAMATERIALNUMBER", DbType.String).Value = model.SERAMaterialNumber;
                //cmd.Parameters.Add("SERAMATERIALDESCRIPTION", DbType.String).Value = model.SERAMaterialDescription;
                //cmd.Parameters.Add("AICOLOR", DbType.String).Value = model.AIColor;
                //cmd.Parameters.Add("SERACOLOR", DbType.String).Value = model.SERAColor;
                //cmd.Parameters.Add("QUOTATIONNO", DbType.String).Value = model.QuotationNo;
                cmd.Parameters.Add("SALESORDERNO", DbType.String).Value = model.SalesOrderNo;
                cmd.Parameters.Add("SALESORDERSTATUS", DbType.Boolean).Value = model.SalesOrderStatus;
                cmd.Parameters.Add("CURRENCY", DbType.String).Value = model.Currency;
                cmd.Parameters.Add("YEAR", DbType.Int32).Value = model.Year;
                cmd.Parameters.Add("BILLINGSTATUS", DbType.Boolean).Value = model.BillingStatus;
                cmd.Parameters.Add("BSTKBNO", DbType.String).Value = model.BSTKBNo;
                cmd.Parameters.Add("REVISISTNK", DbType.DateTime).Value = model.RevisiSTNK;
                cmd.Parameters.Add("NAMAPENERIMA", DbType.String).Value = model.NamaPenerima;
                cmd.Parameters.Add("ALAMATPENERIMA", DbType.String).Value = model.AlamatPenerima;
                //cmd.Parameters.Add("HSORIS", DbType.String).Value = HSorIS;

                OpenConnection();
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                result = 0;
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
            return result;
        }

        public int UpdateCustomPOStatusPOId(string poNumber, string poStatusId)
        {
            int result;
            try
            {
                EntityCommand cmd = new EntityCommand("EProcEntities.sp_UpdateCustomPOStatusPOId", entityConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("PONUMBER", DbType.String).Value = poNumber;
                cmd.Parameters.Add("POSTATUSID", DbType.String).Value = poStatusId;
                OpenConnection();
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                result = 0;
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
            return result;
        }
        #endregion

        #endregion
    }
}