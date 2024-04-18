using HTTPWrapper;
using MTML.GENERATOR;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using static LeSCommon.LeSCommon;

namespace FujianDaQin_Routine
{
    public class FujianDaQin : LeSCommon.LeSCommon
    {



        public string errormsg = "", mailtxt_path = "", rfqmtml_path = "", processorname = "", mailmsgFilename = "'", sDoneFile = "", processedRFQ = "";
        public bool isRFQ = false, isPO = false;
        public string istoreUrl = "", portalUrl = "", username = "", LoginURL = "", password = "", Ref_Url = "", buyercode = "", buyercode1 = "", suppcode = "", buyersupplinkcode = "", buyername = "", clientName = "", suppname = "", server = "", currDocType = "", textfile = "";
        public string sessionid = "", req = "", vesselid = "", vendorid = "", rfqno = "", quotationid = "", referrer = "", headerdetailhtmldata = "", lineitemhtmldata = "", itemrefferer = "", Headeresponsestr = "", cEncryprtLink = "";
        public string Ref_body_data = "", Currency = "", dataLink = "", htmlNode = "", AuditName = "", AttachmentPath = "", cslUrl = "", addressString = "", uniqueKeyValue = "", ref_Number = "", ref_Name = "", rfqUrl = "", origin = "";
        public Dictionary<string, string> dctHeaderDetails = new Dictionary<string, string>();
        public Dictionary<string, string> _dctStateData = new Dictionary<string, string>();
        public Dictionary<string, string> _dctItemDescr = new Dictionary<string, string>();

        string linkFileName = "", mailtextname = "", cEmbdPdfAttachPath = "", cEmbdPdfAttachFile = "", QuotePath = "", UCRefNo = "", AAGRefNo = "", HeaderDiscount = "0", LeadDays = "0", ExpDate = "",
          DelvDate = "", MsgRefNumber = "", QuoteCurrency = "", FreightAmt = "0", OtherCosts = "0", TaxCost = "0", DepositCost = "0", GrandTotal = "0", TotalLineItemsAmt = "0",
          SupplierComment = "", PayTerms = "", TermsCondition = "", BuyerTotal = "0", Allowance = "0", PackingCost = "0", cHSCode = "";
        string[] Actions, _arrByrDet, _arrByrLinkCode, _arrByrSppLinkId;
        private int loadUrlRe = 0, Retry = 0; int count = 0, lineItemCount = 0; int ReQuiredDays = 0;
        public bool IsSubmitQuote = false, IsSendMail = false, IsDecline = false, IsSaveQuote = false, IsUploadAttach = false, IsProcessMailPDF = false, PROCESS_THROUGH_MAIL = false;

        public void StartProcess()
        {
            try
            {
                processorname = ConfigurationManager.AppSettings["PROCESSOR_NAME"];
                rfqmtml_path = ConfigurationManager.AppSettings["LeS_MTML_PATH"];
                AuditPath = ConfigurationManager.AppSettings["ESUPPLIER_AUDIT"] + "\\";
                if (!Directory.Exists(rfqmtml_path)) Directory.CreateDirectory(rfqmtml_path);
                processedRFQ = AppDomain.CurrentDomain.BaseDirectory + "DownloadedRFQ.txt";
                AuditName = Convert.ToString(ConfigurationManager.AppSettings["AUDIT_NAME"].Trim());
                AttachmentPath = PrintScreenPath = Convert.ToString(ConfigurationManager.AppSettings["ESUPPLIER_ATTACHMENTS"].Trim());
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                LoadAppsettingsXML();

            }
            catch (Exception ex)
            {
                LogText = "Error occurred while initialization" + ex.Message;
            }
        }

        private void LoadAppsettingsXML()
        {
            try
            {
                string appSettingFile = Environment.CurrentDirectory + "\\AppSettings.xml";
                if (File.Exists(appSettingFile))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(appSettingFile);

                    XmlNodeList xmlAppSettings = document.SelectNodes("//APPSETTINGS");
                    if (xmlAppSettings != null)
                    {
                        foreach (XmlNode appSetting in xmlAppSettings)
                        {
                            dctAppSettings = new Dictionary<string, string>();
                            XmlNodeList childNodes = appSetting.ChildNodes;
                            foreach (XmlNode setting in childNodes)
                            {
                                XmlElement userSetting = (XmlElement)setting;
                                dctAppSettings.Add(userSetting.Name, userSetting.InnerText);
                            }

                            if (dctAppSettings != null && dctAppSettings.Count > 0)
                            {
                                #region GET SETTINGS
                                portalUrl = dctAppSettings["DOMAIN"];
                                istoreUrl = dctAppSettings["DOMAINNAME"];
                                LoginURL = dctAppSettings["LOGINURL"];
                                dataLink = dctAppSettings["DATALINK"];
                                username = dctAppSettings["USERNAME"];
                                password = dctAppSettings["PASSWORD"];
                                buyercode = dctAppSettings["BUYERCODE"];
                                origin = dctAppSettings["ORIGIN"];
                                suppcode = dctAppSettings["SUPPLIERCODE"];
                                buyersupplinkcode = dctAppSettings["BUYERSUPPLIERLINKCODE"];
                                buyername = dctAppSettings["BUYERNAME"];
                                clientName = dctAppSettings["CLIENTNAME"];
                                suppname = dctAppSettings["SUPPLIERNAME"];
                                ReQuiredDays = Convert.ToInt32(dctAppSettings["REQUIREDDAYS"]);
                                textfile = dctAppSettings["LINK_INBOX_PATH"];
                                PROCESS_THROUGH_MAIL = Convert.ToBoolean(dctAppSettings["PROCESS_THROUGH_MAIL"]);
                                string Actions = dctAppSettings["ACTIONS"];
                                #endregion

                                LogText = "Processing Started";
                                this.URL = portalUrl;

                                _httpWrapper._AddRequestHeaders.Clear();
                                _httpWrapper._SetRequestHeaders.Clear();
                                _httpWrapper.RequestMethod = "GET";
                                bool loadurl = _httpWrapper.LoadURL(this.URL, "", "", "", "");
                                if (loadurl)
                                {
                                    LogText = "Link loaded";
                                    bool login = Login();
                                    if (login)
                                    {
                                        LogText = "Log In Successful";
                                        try
                                        {
                                            string[] docTypes = Actions.Split(',');
                                            foreach (string docType in docTypes)
                                            {
                                                LogText = "Processing " + docType;
                                                switch (docType.ToLower())
                                                {
                                                    case "rfq":
                                                        currDocType = "RFQ";
                                                        isRFQ = true;
                                                        ProcessRFQ();
                                                        break;
                                                    case "quote":
                                                        currDocType = "QUOTE";
                                                        isRFQ = false;
                                                        //ProcessQuote();
                                                        break;
                                                    default: LogText = "Unknown doctype " + docType; break;
                                                }
                                                LogText = "--------*Process for " + currDocType + " is Done*--------*--------";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogText = ex.Message;
                                            throw ex;
                                        }
                                    }
                                    else
                                    {
                                        LogText = "Login Failed";
                                        CreateAuditFile("", processorname, "", "ERROR", "LeS:1004 - Login Failed, Unable to process RFQ", buyercode, suppcode, AuditPath);

                                    }
                                }
                                else
                                {
                                    errormsg = "Unable to Load URL -" + portalUrl;
                                    LogText = errormsg;
                                }

                            }
                            else
                            {
                                errormsg = "Content in AppSettings.xml is Null";
                                LogText = errormsg;
                            }
                        }
                    }
                    else
                    {
                        errormsg = "Content in AppSettings.xml is Null";
                        LogText = errormsg;
                    }
                }
                else
                {
                    errormsg = "AppSettings.xml file not found";
                    LogText = errormsg;
                }
            }
            catch (Exception ex)
            {
                LogText = "Error occurred while reading AppSettings xml -" + ex.Message;
                CreateAuditFile("", processorname, "", "ERROR", "Error occurred while reading AppSettings xml -" + ex.Message, buyercode, suppcode, AuditPath);
            }
        }

        private bool Login()
        {
            bool login = false;
            try
            {

                string body = $"{{lang=CN&uid={username}&pwd={password}&saveUid=true&savePwd=true&needVerify=false&verifyCode=";

                referrer = _httpWrapper._CurrentResponse.ResponseUri.OriginalString;
                _httpWrapper.Referrer = referrer;
                _httpWrapper.RequestMethod = "POST";

                _httpWrapper._AddRequestHeaders.Add("origin", origin);
                _httpWrapper.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                _httpWrapper.AcceptMimeType = "*/*";



                bool loginResult = _httpWrapper.PostURL(LoginURL, body, "", "", "");

                if (loginResult)
                {
                    LogText = "logged in";

                    string responce = _httpWrapper._CurrentResponseString;

                    login = true;
                }
                else
                {
                    LogText = "Unable to Login";
                    login = false;
                }
            }
            catch (Exception ex)
            {
                LogText = "Error while Login " + ex.Message;
                CreateAuditFile(mailmsgFilename, processorname, rfqno, "ERROR", "Unable to Login " + ex.Message, buyercode, suppcode, AuditPath);
                throw ex;
            }
            return login;
        }


        public string GetRFQNO()
        {
            try
            {
                DirectoryInfo _dir = new DirectoryInfo(textfile);
                if (_dir.GetFiles().Length > 0)
                {
                    FileInfo[] _Files = _dir.GetFiles();
                    foreach (FileInfo f in _Files)
                    {
                        string cMSgFile = File.ReadAllText(f.FullName);
                        string RFQNO = GetRFQNO_TextFile(f.FullName,f.Name);

                        if (!string.IsNullOrWhiteSpace(RFQNO))
                        {

                            return RFQNO;
                           

                        }
                        else
                        {
                            LogText = "link not found in this file:" + f.FullName;
                            CreateAuditFile(f.FullName, processorname, "", "Error", "link not found :" + f.FullName, buyercode, suppcode, AuditPath);
                            MoveFiles(f.FullName, textfile + "\\Error\\" + Path.GetFileName(f.FullName));
                        }
                    }
                    return "";

                }
                else
                {
                    LogText = "No files found.";
                    return "";
                }
            }catch (Exception ex)
            {
                LogText = "Error while Extrating RFQ number from Mail file :" + ex.Message;
                return "";
            }

        }


        public string GetRFQNO_TextFile(string filepath, string Filename)
        {
            string RFQNO = "";
            try
            {
                string fileContent = File.ReadAllText(filepath);
                //string pattern = @"HT.*?(?=\s|<|$)";
                string pattern = @"\bHT\d{2}[A-Z]{3}-\d{5}-\d{4}\b";
                HashSet<string> uniqueMatches = new HashSet<string>();

                MatchCollection matches = Regex.Matches(fileContent, pattern);

                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        uniqueMatches.Add(match.Value);
                    }

                    foreach (string uniqueMatch in uniqueMatches)
                    {
                        RFQNO = uniqueMatch;
                    }
                }
                else
                {
                    //LogText = "RFQNO NOT FOUND IN EMAIL FILE " + Filename;
                }


                //foreach (Match match in matches)
                //{
                //    url = match.Groups[1].Value;
                //    //linkfromfile = url;
                //    break;

                //}

            }
            catch (Exception ex)
            {
                LogText = "Not getting RFQNO from file...";
                throw new Exception("Not getting RFQNO from file :" + ex.Message);
            }
            return RFQNO;
        }

        public string GetDataAfterLogin()
        {

            try
            {

                string Linkk = "http://htsm.fjhighton.com/data/list/purEnquireMain?_dc=1709531524800&enquireNo=&vesselCode=&supplierCode=WK00000115&bizType=&documentType=&orderType=&status=&enquireDate.from=&enquireDate.to=&getPriceNo=&_extend=---AFA8AI669JA295A2A39J668J8K665K69706K6962697074607569626970746072695L66958K98669JA295A2A39J668J8K665K69706K696269706L69626970746962697074607569626970746072696269707569626970726962697073696269706E69626971756962696K736962696K6E6962696L736962696L6E695L&searchType=BJD_MAIN&dataType=2&_sm=0&_lm=a&dataType.eq=2&_od=createdDtmLoc%20desc&_cpid=8a9282cd798a11e201799d776eff0207&page=1&start=0&limit=50";


                referrer = _httpWrapper._CurrentResponse.ResponseUri.OriginalString;
                _httpWrapper.Referrer = referrer;

                _httpWrapper.AcceptMimeType = "*/*";



                bool loadurl = _httpWrapper.LoadURL(Linkk, "", "", "", "");


                if (loadurl)
                {

                    string responce = _httpWrapper._CurrentResponseString;



                    return responce;
                }
                else
                {
                    LogText = "Unable to get RFQ Data";
                    return "";
                }

            }
            catch (Exception ex)
            {
                LogText = "Error occurred while GetDataAfterLogin() " + ex.Message;
                CreateAuditFile(mailmsgFilename, processorname, "", "ERROR", "Unable to Get Data After Login" + errormsg, suppcode, buyercode, AuditPath);
                return "";
            }
        }



        public string GetDATA(string enquireMainId, string status, string enquireNo)
        {

            try
            {
                if (status == "02" || status == "04.6" || status == "04.5") {


                    string keyString = $" and enquireMainId = '{enquireMainId}' and isDelete = '0' ";


                    string extend = GetExtendParameter(keyString);

                    string Linkk = "http://htsm.fjhighton.com/data/list/purEnquireDetail?_dc=1709548670253&_extend="+extend+"&_sm=0&_lm=a&_od=seqNo%2CequipCode%2CpartsNo%2CsupplierCode&_cpid=8a9282cd798a11e201799d776eff0207&page=1&start=0&limit=500&group=%5B%7B%22property%22%3A%22groupType%22%2C%22direction%22%3A%22ASC%22%7D%5D&sort=%5B%7B%22property%22%3A%22groupType%22%2C%22direction%22%3A%22ASC%22%7D%5D";

                   
                    referrer = _httpWrapper._CurrentResponse.ResponseUri.OriginalString;
                    _httpWrapper.Referrer = referrer;
                   

                    _httpWrapper.AcceptMimeType = "*/*";

                    bool loadurl = _httpWrapper.LoadURL(Linkk, "", "", "", "");


                    if (loadurl)
                    {

                        string responce = _httpWrapper._CurrentResponseString;

                        return responce;
                    }
                    else
                    {
                        LogText = "Unable to get RFQ Data";
                        return "";
                    }

                }
                else
                {
                    LogText = "Status Has Changed";
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogText = "Error occurred while GetDATA() " + ex.Message;
                CreateAuditFile(mailmsgFilename, processorname, "", "ERROR", "Unable to Get Data for  Rfq No"+ enquireNo  + errormsg, suppcode, buyercode, AuditPath);
                return "";
            }
        }


        private void ProcessRFQ()
        {
            //LogText = "RFQ generation Started";
            referrer = ""; Ref_Url = "";

            string Link = "";
            string RfqNo = "";
            bool gerenerated = false;



            try
            {
                Root DATA = new Root();
                String data = GetDataAfterLogin();


                if (data != null)
                {

                    dynamic json = JsonConvert.DeserializeObject(data);

                    if (!PROCESS_THROUGH_MAIL)
                    {


                        foreach (var item in json.rows)
                        {
                            string enquireMainId = item.enquireMainId;
                            string status = item.status;
                            string enquireNo = item.enquireNo;


                            string downloadRFQPath = Environment.CurrentDirectory + "\\" + "DownloadedRFQ.txt";
                            Boolean aa = File.ReadAllLines(downloadRFQPath).Contains(enquireNo);
                            if (!File.Exists(downloadRFQPath) || !File.ReadAllLines(downloadRFQPath).Contains(enquireNo))
                            {

                                count++;
                                LogText = count + " : " + " Started RFQ generation for " + enquireNo;


                                gerenerated = GenerateRFQMtml(status, enquireNo, enquireMainId, item,"", PROCESS_THROUGH_MAIL);


                            }
                            else
                            {
                                LogText = "RFQ " + enquireNo + " is already created";
                                CreateAuditFile("", processorname, "", "Error", "RFQ for :" + enquireNo +" is already created :", buyercode, suppcode, AuditPath);

                            }
                        }
                    }
                    else
                    {

                        try
                        {
                            DirectoryInfo _dir = new DirectoryInfo(textfile);
                            if (_dir.GetFiles().Length > 0)
                            {
                                FileInfo[] _Files = _dir.GetFiles();
                                foreach (FileInfo f in _Files)
                                {
                                    string cMSgFile = File.ReadAllText(f.FullName);
                                    string RFQNO = GetRFQNO_TextFile(f.FullName,f.Name);

                                    if (!string.IsNullOrWhiteSpace(RFQNO))
                                    {

                                        bool isPresent = false;

                                        foreach (var item in json.rows)
                                        {
                                            string enquireMainId = item.enquireMainId;
                                            string status = item.status;
                                            string enquireNo = item.enquireNo;

                                            if (enquireNo == RFQNO)
                                            {
                                                isPresent = true;
                                            }

                                            if (isPresent) { 
                                                string downloadRFQPath = Environment.CurrentDirectory + "\\" + "DownloadedRFQ.txt";
                                                Boolean aa = File.ReadAllLines(downloadRFQPath).Contains(enquireNo);
                                                //if (!File.Exists(downloadRFQPath) || !File.ReadAllLines(downloadRFQPath).Contains(enquireNo))
                                                //{

                                                    count++;
                                                    LogText = count + " : " + " Started RFQ generation for " + enquireNo;


                                                    gerenerated = GenerateRFQMtml(status, enquireNo, enquireMainId, item, f.Name, PROCESS_THROUGH_MAIL);


                                                //}
                                                //else
                                                //{
                                                //    LogText = "RFQ " + enquireNo + " is already created";
                                                //}
                                            }
                                  
                                        }

                                        if (!isPresent)
                                        {
                                            LogText = "RFQNO :" + RFQNO + " From mail file not available";
                                            CreateAuditFile(f.Name, processorname, "", "Error", "RFQNO :" + RFQNO + " From mail file not available " + f.Name, buyercode, suppcode, AuditPath);
                                            MoveFiles(f.FullName, textfile + "\\Error\\" + Path.GetFileName(f.FullName));

                                        }



                                    }
                                    else
                                    {
                                        LogText = "RFQNO not found in this file:" + f.FullName;
                                        CreateAuditFile(f.Name, processorname, "", "Error", "RFQNO not found :" + f.Name, buyercode, suppcode, AuditPath);
                                        MoveFiles(f.FullName, textfile + "\\Error\\" + Path.GetFileName(f.FullName));
                                    }
                                }

                            }
                            else
                            {
                                LogText = "No files found.";
                            }
                        }
                        catch (Exception ex)
                        {
                            LogText = "Error while Extrating RFQ number from Mail file :" + ex.Message;
                        }

                    }

                }
                else
                {
                    LogText = "RFQ Data Not Found";
                }
            }
            catch (Exception ex)
            {
                LogText = "Error occurred while ProcessRFQ() " + ex.Message;
                CreateAuditFile(mailmsgFilename, processorname, "", "ERROR", " Error occurred while ProcessRFQ() " + errormsg, suppcode, buyercode, AuditPath);
                if (!Directory.Exists(mailtxt_path + "\\" + "Error")) Directory.CreateDirectory(mailtxt_path + "\\" + "Error");
                File.Move(mailmsgFilename, mailtxt_path + "\\" + "Error\\" + Path.GetFileName(mailmsgFilename));
            }
        }


       




        public string GetExtendParameter(string e)
        {
            if (e.IndexOf("---") == 0)
            {
                return e;
            }

            string d = "";
            for (int b = 0; b < e.Length; b++)
            {
                int c = (int)e[b];

                if (c > 255)
                {
                    d += "Z" + A() + ConvertToBase22(c) + "X" + A();
                }
                else
                {
                    string f = ConvertToBase22(c ^ 170).ToUpper();
                    d += f.Length == 1 ? "0" + f : f;
                }
            }

            return "---" + d;
        }

        public string ConvertToBase22(int number)
        {
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";

            do
            {
                result = characters[number % 22] + result;
                number /= 22;
            } while (number > 0);

            return result;
        }

        public int A()
        {
            Random random = new Random();
            return random.Next(1, 10);
        }



        private bool GenerateRFQMtml(string status, string enquireNo,string enquireMainId, dynamic json, string Filename,bool PROCESS_THROUGH_MAIL)
        {
            bool result = false;
            try
            {
                LogText = "Start filling HeaderData in MTML";
                MTMLInterchange interchange = new MTMLInterchange();
                DocHeader docHeader = new DocHeader();

                

                //Header 

                docHeader.VersionNumber = "1";
                docHeader.DocType = "RequestForQuote";
                //docHeader.CurrencyCode = convert.ToString(json.currencyName);
                interchange.DocumentHeader = docHeader;
                interchange.Sender = buyersupplinkcode;
                interchange.Recipient = suppcode;
                interchange.ControlReference = DateTime.Now.ToString("yyyyMMddHHmmss");
                interchange.Identifier = DateTime.Now.ToString("yyyyMMddHHmmss");
                interchange.PreparationDate = DateTime.Now.ToString("yyyy-MMM-dd");
                interchange.PreparationTime = DateTime.Now.ToString("HH:mm");
                docHeader.MessageReferenceNumber = enquireNo;
                if (Filename != "") 
                { 
                    docHeader.MessageNumber = Filename; 
                }
                if (Filename != "")
                {
                    docHeader.OriginalFile = Filename;
                }


                docHeader.References.Add(new Reference(ReferenceQualifier.UC, enquireNo));

                DateTime dt = DateTime.MinValue;
                DateTimePeriod dtDocDate = new DateTimePeriod();
                dt = convert.ToDateTime(json.enquireDate);
                dtDocDate.FormatQualifier = DateTimeFormatQualifiers.CCYYMMDD_102;
                dtDocDate.Qualifier = DateTimePeroidQualifiers.DocumentDate_137;

                if (dt != DateTime.MinValue)
                {
                    dtDocDate.Value = dt.ToString("yyyyMMdd");
                    docHeader.DateTimePeriods.Add(dtDocDate);
                }
                else { dt = DateTime.MinValue; }

                DateTime dt1 = DateTime.MinValue;
                DateTimePeriod dtDocDate1 = new DateTimePeriod();
                dt1 = convert.ToDateTime(json.effDateTo);
                dtDocDate1.FormatQualifier = DateTimeFormatQualifiers.CCYYMMDD_102;
                dtDocDate1.Qualifier = DateTimePeroidQualifiers.LatestDeliveryDate_2;

                if (dt1 != DateTime.MinValue)
                {
                    dtDocDate1.Value = dt1.ToString("yyyyMMdd");
                    docHeader.DateTimePeriods.Add(dtDocDate1);
                }
                else { dt1 = DateTime.MinValue; }



                //add any header comment here
                Comments _HderRmrks = new Comments();
                _HderRmrks.Qualifier = CommentTypes.PUR;
                _HderRmrks.Value = convert.ToString(json.remark);
                docHeader.Comments.Add(_HderRmrks);


                docHeader.PartyAddresses = GetPartyAddress(json);

                string data1 = GetDATA(enquireMainId, status, enquireNo);
                JObject jsonLineData = JObject.Parse(data1);



                docHeader.LineItems = GetLineItems( jsonLineData);
                

                docHeader.LineItemCount = docHeader.LineItems.Count;
                if (docHeader.LineItemCount == 0)
                {
                    throw new Exception("Lineitem count is zero");
                }

                string fileName = rfqmtml_path + "\\" + "MTML_RFQ_"+convert.ToFileName(Filename.Replace(".txt",""))+ "_" +convert.ToFileName(enquireNo) + "_" + buyercode + "_" + suppcode + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xml";

                MTMLClass _class = new MTMLClass();
                _class.Create(interchange, fileName);
                LogText = "RFQ Mtml Generated for " + enquireNo;
                CreateAuditFile(fileName, processorname, enquireNo, "SUCCESS", "RFQ Generated Successfully for " + enquireNo, suppcode, buyercode, AuditPath);

                if(PROCESS_THROUGH_MAIL)
                {
                    MoveFiles(Filename, textfile + "\\SUCCESS\\" + Path.GetFileName(Filename));

                }

                if (!PROCESS_THROUGH_MAIL)
                {
                    File.AppendAllText(Environment.CurrentDirectory + "\\" + "DownloadedRFQ.txt", enquireNo + Environment.NewLine);
                }
                result = true;
            }
            catch (Exception ex)
            {
                LogText = "Error while Generating MTML " + ex.Message;
                throw ex;
            }
            return result;
        }

        private LineItemCollection GetLineItems( JObject jsonLineData)
        {


            LogText = "Start filling LineItems in MTML";
            LineItemCollection lineItems = new LineItemCollection();
            LineItem lineItem = new LineItem();


            int dataLength = jsonLineData["rows"].Count();
            lineItemCount = 0;



            try
            {
                for (int i = 0; i < dataLength; i++, lineItemCount++)
                {
                    lineItem = new LineItem();
                    lineItem.Number = convert.ToString((lineItemCount + 1));


                    lineItem.Description = convert.ToString(jsonLineData["rows"][i]["partsName"]);
                    lineItem.SYS_ITEMNO = (i + 1);
                    lineItem.Identification = convert.ToString(jsonLineData["rows"][i]["partsNo"]);

                    lineItem.Quantity = (double)jsonLineData["rows"][i]["inquiryQty"];
                    lineItem.MeasureUnitQualifier = convert.ToString(jsonLineData["rows"][i]["unitName"]);

                    lineItem.LineItemComment = new Comments();
                    lineItem.LineItemComment.Qualifier = CommentTypes.LIN;
                    lineItem.LineItemComment.Value = convert.ToString(jsonLineData["rows"][i]["model"]);
                    //lineItem.Section = new Section();
                    //lineItem.Section.ModelNumber = convert.ToString(jsonLineData["rows"][i]["model"]);


                    lineItems.Add(lineItem);
                }
            }
            catch (Exception e)
            {
                LogText = "Error in Filling Items in MTML " + e.Message;
                throw e;
            }
            return lineItems;
        }

        private PartyCollection GetPartyAddress(dynamic json)
        {
            PartyCollection collection = new PartyCollection();
            collection.Clear();
            try
            {
                LogText = "Start filling PartyAddress in MTML";

                //Vendor
                Party vendor = new Party();
                vendor.Name = suppname;
                vendor.Qualifier = PartyQualifier.VN;
                collection.Add(vendor);

                //Buyer 
                Party buyer = new Party();
                buyer.Qualifier = PartyQualifier.BY;
                buyer.Name = buyername;
                collection.Add(buyer);

                //Vessel
                Party vessel = new Party();
                vessel.Qualifier = PartyQualifier.UD;
                vessel.Name = convert.ToString(json.vesselName);
                collection.Add(vessel);
            }
            catch (Exception ex)
            {
                LogText = "Error while Filling Party Address in  MTML " + ex.Message;
                throw ex;
            }

            return collection;
        }



    }
}
