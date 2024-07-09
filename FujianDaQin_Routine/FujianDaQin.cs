using HTTPWrapper;
using MTML.GENERATOR;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;

using static LeSCommon.LeSCommon;
using FILE_Conversion;
using System.Net.Mime;
using System.Security.Policy;

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

        HttpWebResponse webResponse = null;



        string linkFileName = "", mailtextname = "", cEmbdPdfAttachPath = "", UPLOADFILEURL= "", LINEITEMPOSTURL= "", GETDATALINK= "", SAVEQUOTEURL= "", cEmbdPdfAttachFile = "", QuotePath = "", UCRefNo = "", AAGRefNo = "", HeaderDiscount = "0", LeadDays = "0", ExpDate = "", DocDate = "",
          DelvDate = "", MsgRefNumber = "", QuoteCurrency = "", FreightAmt = "0", OtherCosts = "0", TaxCost = "0", DepositCost = "0", GrandTotal = "0", TotalLineItemsAmt = "0",
          SupplierComment = "", PayTerms = "", TermsCondition = "", REQComment = "", BuyerTotal = "0", Allowance = "0", PackingCost = "0", cHSCode = "";
        string[] Actions, _arrByrDet, _arrByrLinkCode, _arrByrSppLinkId;
        private int loadUrlRe = 0, Retry = 0; int count = 0, lineItemCount = 0; int ReQuiredDays = 0;
        public bool IsSubmitQuote = false, IsSendMail = false, IsDecline = false, IsSaveQuote = false, IsUploadAttach = false, IsProcessMailPDF = false, PROCESS_THROUGH_MAIL = false;

        MTMLInterchange _interchange { get; set; }
        public LineItemCollection _QuoteLineItems = null;
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
                                GETDATALINK = dctAppSettings["GETDATALINK"];
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
                                QuotePath = dctAppSettings["OUTBOX_PATH"];
                                cEmbdPdfAttachPath = dctAppSettings["QUOTE_ATTACHMENT"];
                                //GETDATALINK = dctAppSettings["GETDATALINK"];
                                SAVEQUOTEURL = dctAppSettings["SAVEQUOTEURL"];
                                UPLOADFILEURL = dctAppSettings["UPLOADFILEURL"];
                                LINEITEMPOSTURL = dctAppSettings["LINEITEMPOSTURL"];
                                string Actions = dctAppSettings["ACTIONS"];
                                IsSubmitQuote = Convert.ToBoolean(dctAppSettings["SUBMIT_QUOTE"]);
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
                                                        ProcessQuote();
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

                if (RFQNO == "")
                {
                    //string fileContent = File.ReadAllText(filepath);
                    string pattern2 = @"\bHT\d{2}[A-Z]{3}-\d{5}\b";
                    HashSet<string> uniqueMatches2 = new HashSet<string>();

                    MatchCollection matches2 = Regex.Matches(fileContent, pattern2);

                    if (matches2.Count > 0)
                    {
                        foreach (Match match in matches2)
                        {
                            string matchedValue = match.Value;
                            if (!uniqueMatches.Contains(matchedValue))
                            {
                                uniqueMatches2.Add(matchedValue);

                                if (!matchedValue.EndsWith("-0000"))
                                {
                                    RFQNO = matchedValue + "-0000";
                                }
                                else
                                {
                                    RFQNO = matchedValue;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Handle case when no matches are found
                        // LogText = "RFQNO NOT FOUND IN EMAIL FILE " + Filename;
                    }

                }
                

            }
            catch (Exception ex)
            {
                LogText = "Not getting RFQNO from file...";
                throw new Exception("Not getting RFQNO from file :" + ex.Message);
            }
            return RFQNO;
        }

        //CHECKING FOR GIT CHANGES

        public string GetDataAfterLogin()
        {

            try
            {

                //string Linkk = "http://htsm.fjhighton.com/data/list/purEnquireMain?_dc=1709531524800&enquireNo=&vesselCode=&supplierCode=WK00000115&bizType=&documentType=&orderType=&status=&enquireDate.from=&enquireDate.to=&getPriceNo=&_extend=---AFA8AI669JA295A2A39J668J8K665K69706K6962697074607569626970746072695L66958K98669JA295A2A39J668J8K665K69706K696269706L69626970746962697074607569626970746072696269707569626970726962697073696269706E69626971756962696K736962696K6E6962696L736962696L6E695L&searchType=BJD_MAIN&dataType=2&_sm=0&_lm=a&dataType.eq=2&_od=createdDtmLoc%20desc&_cpid=8a9282cd798a11e201799d776eff0207&page=1&start=0&limit=50";
                string Linkk = GETDATALINK;

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
                //Root DATA = new Root();
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


                                gerenerated = GenerateRFQMtml(status, enquireNo, enquireMainId, item,"","", PROCESS_THROUGH_MAIL);


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
                                                //Boolean aa = File.ReadAllLines(downloadRFQPath).Contains(enquireNo);
                                                //if (!File.Exists(downloadRFQPath) || !File.ReadAllLines(downloadRFQPath).Contains(enquireNo))
                                                //{

                                                    count++;
                                                    LogText = count + " : " + " Started RFQ generation for " + enquireNo;


                                                    gerenerated = GenerateRFQMtml(status, enquireNo, enquireMainId, item, f.Name,f.FullName, PROCESS_THROUGH_MAIL);
                                                    break;

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



        private bool GenerateRFQMtml(string status, string enquireNo,string enquireMainId, dynamic json, string Filename,string fullFileName,bool PROCESS_THROUGH_MAIL)
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
                    MoveFiles(fullFileName, textfile + "\\SUCCESS\\" + Path.GetFileName(Filename));

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
                    lineItem.UDF1 = convert.ToString(jsonLineData["rows"][i]["partsInfoId"]);

                    lineItem.LineItemComment = new Comments();
                    lineItem.LineItemComment.Qualifier = CommentTypes.LIN;
                    lineItem.LineItemComment.Value = convert.ToString(jsonLineData["rows"][i]["model"])+ ",  "+ convert.ToString(jsonLineData["rows"][i]["applyUserTel"])+",  "+ convert.ToString(jsonLineData["rows"][i]["remark"]) ;
         
                    //lineItem.LineItemComment.Value = convert.ToString(jsonLineData["rows"][i]["applyUserTel"]);
                  
                    //lineItem.LineItemComment.Value = convert.ToString(jsonLineData["rows"][i]["remark"]);

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






        private void ProcessQuote()
        {
            try
            {
                LogText = "Quote processing Started";

                DirectoryInfo _dir = new DirectoryInfo(QuotePath);
                FileInfo[] _Files = _dir.GetFiles();
                if (_Files.Length > 0)
                {
                    foreach (FileInfo _MtmlFile in _Files)
                    {
                        if (_MtmlFile.FullName.Contains("QUOTE")) { ProcessQuotationDetail(_MtmlFile.FullName); }
                    }
                }
                else
                {
                    LogText = "No Quote files found to process.";
                }
                LogText = "Quote processing Stopped.";
            }
            catch (Exception ex)
            {
                LogText = "Exception while processing files : " + ex.GetBaseException().ToString();
            }
        }

        public void LoadInterchangeDetails(string xmlQuoteFile)
        {
            try
            {
                MTMLClass _mtml = new MTMLClass();
                _interchange = _mtml.Load(xmlQuoteFile);

                if (_interchange != null)
                {
                    if (_interchange != null && _interchange.DocumentHeader != null && _interchange.DocumentHeader.IsDeclined != null)
                    {
                        IsDecline = _interchange.DocumentHeader.IsDeclined;
                    }
                    for (int g = 0; g < _interchange.DocumentHeader.References.Count; g++)
                    {
                        if (_interchange.DocumentHeader.References[g].Qualifier == ReferenceQualifier.UC)
                        {
                            UCRefNo = _interchange.DocumentHeader.References[g].ReferenceNumber.Trim();
                        }
                        if (_interchange.DocumentHeader.References[g].Qualifier == ReferenceQualifier.AAG)
                        {
                            AAGRefNo = _interchange.DocumentHeader.References[g].ReferenceNumber.Trim();
                        }
                    }
                    if (_interchange.DocumentHeader.AdditionalDiscount != null && convert.ToFloat(_interchange.DocumentHeader.AdditionalDiscount) > 0)
                    {
                        HeaderDiscount = Convert.ToString(_interchange.DocumentHeader.AdditionalDiscount);
                        if (string.IsNullOrWhiteSpace(HeaderDiscount)) HeaderDiscount = "0.00";
                    }
                    if (_interchange.DocumentHeader.MessageReferenceNumber != null)
                    {
                        MsgRefNumber = _interchange.DocumentHeader.MessageReferenceNumber;
                    }

                    #region -- Read Interchange Object

                    if (_interchange.DocumentHeader.LeadTimeDays != null) { LeadDays = _interchange.DocumentHeader.LeadTimeDays; }

                    QuoteCurrency = Convert.ToString(_interchange.DocumentHeader.CurrencyCode);


                    #region -- Read Iterchange Time Period

                    for (int g = 0; g < _interchange.DocumentHeader.DateTimePeriods.Count; g++)
                    {
                        if (_interchange.DocumentHeader.DateTimePeriods[g].Qualifier != null)
                        {
                            if (_interchange.DocumentHeader.DateTimePeriods[g].Qualifier == DateTimePeroidQualifiers.DocumentDate_137)
                            {
                                if (_interchange.DocumentHeader.DateTimePeriods[g].Value != null)
                                {
                                    DateTime dtDocumentdate = FormatMTMLDate(_interchange.DocumentHeader.DateTimePeriods[g].Value);
                                    if (dtDocumentdate != DateTime.MinValue)
                                    {
                                        DocDate = dtDocumentdate.ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                }
                            }
                            if (_interchange.DocumentHeader.DateTimePeriods[g].Qualifier == DateTimePeroidQualifiers.ExpiryDate_36)
                            {
                                if (_interchange.DocumentHeader.DateTimePeriods[g].Value != null)
                                {
                                    DateTime dtExpredate = FormatMTMLDate(_interchange.DocumentHeader.DateTimePeriods[g].Value);
                                    if (dtExpredate != DateTime.MinValue)
                                    {
                                        ExpDate = dtExpredate.ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                }
                            }
                            if (_interchange.DocumentHeader.DateTimePeriods[g].Qualifier == DateTimePeroidQualifiers.DeliveryDate_69)
                            {
                                if (_interchange.DocumentHeader.DateTimePeriods[g].Value != null)
                                {
                                    if (_interchange.DocumentHeader.DateTimePeriods[g].Value != null)
                                    {
                                        DateTime DtDelevDate = FormatMTMLDate(_interchange.DocumentHeader.DateTimePeriods[g].Value);
                                        if (DtDelevDate != DateTime.MinValue)
                                        {
                                            DelvDate = DtDelevDate.ToString("yyyy-MM-ddTHH:mm:ss");
                                        }

                                    }
                                }
                            }
                        }
                    }

                    #endregion


                    #region -- Supplier Comment
                    for (int g = 0; g < _interchange.DocumentHeader.Comments.Count; g++)
                    {
                        if (_interchange.DocumentHeader.Comments[g].Qualifier == CommentTypes.SUR)
                        {
                            SupplierComment = (_interchange.DocumentHeader.Comments[g].Value).Trim();
                        }
                        else if (_interchange.DocumentHeader.Comments[g].Qualifier == CommentTypes.ZTP)
                        {
                            PayTerms = _interchange.DocumentHeader.Comments[g].Value;
                        }
                        else if (_interchange.DocumentHeader.Comments[g].Qualifier == CommentTypes.ZTC)
                        {
                            TermsCondition = _interchange.DocumentHeader.Comments[g].Value;
                        }
                        else if (_interchange.DocumentHeader.Comments[g].Qualifier == CommentTypes.ZAT)
                        {
                            REQComment = _interchange.DocumentHeader.Comments[g].Value;
                        }
                    }

                    #endregion

                    _QuoteLineItems = _interchange.DocumentHeader.LineItems;

                    /*for(int i=0; i< _QuoteLineItems.Count; i++)
                    {
                        LineItem line = _QuoteLineItems[i];
                        LogText = "UnitPrice : " + line.DocQtPrice;
                        LogText = "Comment : " + line.LineItemComment.Value;
                    }*/


                    //LogText = "Section : " + line.Section.Name;

                    #region -- Read Iterchange MonetoryAmounts
                    if (_interchange != null && _interchange.DocumentHeader != null && _interchange.DocumentHeader.MonetoryAmounts != null)
                    {
                        for (int g = 0; g < _interchange.DocumentHeader.MonetoryAmounts.Count; g++)
                        {
                            if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.GrandTotal_259)
                            {
                                GrandTotal = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.AllowanceAmount_204)
                            {
                                Allowance = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.FreightCharge_64)
                            {
                                FreightAmt = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.PackingCost_106)
                            {
                                PackingCost = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.TaxCost_99)
                            {
                                TaxCost = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.Deposit_97)
                            {
                                DepositCost = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.OtherCost_98)
                            {
                                OtherCosts = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.BuyerItemTotal_90)
                            {
                                BuyerTotal = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                            else if (_interchange.DocumentHeader.MonetoryAmounts[g].Qualifier == MonetoryAmountQualifier.TotalLineItemsAmount_79)
                            {
                                TotalLineItemsAmt = _interchange.DocumentHeader.MonetoryAmounts[g].Value.ToString();
                            }
                        }
                    }

                    #endregion

                    ReadAttachments(xmlQuoteFile, _interchange.DocumentHeader);

                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error on Load Interchange Details : " + ex);
            }
        }

        private void ReadAttachments(string fXML, DocHeader obj)
        {
            try
            {
                string _outstr = "", cFileName = "", cAttchNode = "";
                int n = 0;
                n = GetNodeCount(fXML, @"MTML/Interchange/" + obj.DocType + "/Attachment");
                if (n > 0) cAttchNode = @"MTML/Interchange/" + obj.DocType + "/Attachment";
                else
                {
                    n = GetNodeCount(fXML, @"MTML/Interchange/Quote/Attachment");
                    cAttchNode = @"MTML/Interchange/Quote/Attachment";
                }

                Dictionary<string, string> _Attachments = new Dictionary<string, string>();

                for (int i = 0; i < n; i++)
                {
                    FileConverter _fileConvert = new FileConverter();
                    _Attachments = GetNodeData(fXML, cAttchNode, i);

                    _Attachments.TryGetValue("Attachment", out _outstr);
                    if (!string.IsNullOrEmpty(_outstr)) { _fileConvert.Base64 = _outstr; }

                    _Attachments.TryGetValue("FileName", out _outstr);
                    if (!string.IsNullOrEmpty(_outstr))
                    {
                        cFileName = convert.ToFileName(_outstr.Trim());
                        _fileConvert.FileName = cEmbdPdfAttachFile = cEmbdPdfAttachPath + "\\" + cFileName;
                    }
                    _Attachments.TryGetValue("format", out _outstr);
                    if (!string.IsNullOrEmpty(_outstr)) { _fileConvert.Format = _outstr; }

                    if (_fileConvert.ExportBase64ToFile())
                    {
                        if (i == 0) obj.OriginalFile = cFileName;
                        else if (i == 1) obj.Attachment1 = cFileName;
                        else if (i == 2) obj.Attachment2 = cFileName;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int GetNodeCount(string fXML, string ANode)
        {
            int _result = 0;
            XmlDocument xd = new XmlDocument();
            xd.Load(fXML);

            XmlNodeList xnList = xd.SelectNodes(ANode);
            if (xnList != null)
            {
                _result = xnList.Count;
            }

            return _result;
        }

        public Dictionary<string, string> GetNodeData(string fXML, string ANode, int AIndex)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
            XmlDocument xd = new XmlDocument();
            xd.Load(fXML);

            XmlNodeList xnList = xd.SelectNodes(ANode);
            if (xnList != null)
            {
                XmlNode _node = xnList.Item(AIndex);
                if (_node != null)
                {
                    _result.Add(_node.Name, _node.InnerText);
                    if (_node.Attributes.Count > 0)
                    {
                        for (int i = 0; i < _node.Attributes.Count; i++)
                        {
                            _result.Add(_node.Attributes[i].Name, _node.Attributes[i].Value);
                        }
                    }

                    for (int j = 0; j < _node.ChildNodes.Count; j++)
                    {
                        GetChildNodeData(fXML, _node.ChildNodes[j], j, _result);
                    }
                }
            }
            return _result;
        }

        public void GetChildNodeData(string fXML, XmlNode ANode, int AIndex, Dictionary<string, string> ADictionary)
        {
            if (ANode != null)
            {
                if ((ANode.HasChildNodes))
                {
                    if (!(ADictionary.ContainsKey(ANode.Name)))
                        ADictionary.Add(ANode.Name, ANode.InnerText);
                }
                else
                {
                    if (ANode.Name != "#text")
                    {
                        if (!(ADictionary.ContainsKey(ANode.Name)))
                            ADictionary.Add(ANode.Name, ANode.Value);
                    }
                }
                if ((ANode.Attributes != null) && (ANode.Attributes.Count > 0))
                {
                    for (int i = 0; i < ANode.Attributes.Count; i++)
                    {
                        if (!(ADictionary.ContainsKey(ANode.Attributes[i].Name)))
                            ADictionary.Add(ANode.Attributes[i].Name, ANode.Attributes[i].Value);
                    }
                }

                for (int j = 0; j < ANode.ChildNodes.Count; j++)
                {
                    if (ANode.ChildNodes[j].Name != "#text")
                    {
                        GetChildNodeData(fXML, ANode.ChildNodes[j], j, ADictionary);
                    }
                }
            }
        }

        public DateTime FormatMTMLDate(string DateValue)
        {
            DateTime Dt = DateTime.MinValue;
            if (DateValue != null && DateValue != "")
            {
                if (DateValue.Length > 5)
                {
                    int year = Convert.ToInt32(DateValue.Substring(0, 4));
                    int Month = Convert.ToInt32(DateValue.Substring(4, 2));
                    int day = Convert.ToInt32(DateValue.Substring(6, 2));
                    Dt = new DateTime(year, Month, day);
                }
            }
            return Dt;
        }

        private void ProcessQuotationDetail(string MTMLFile)
        {
           
            try
            {
                LoadInterchangeDetails(MTMLFile);


                string data = GetDataAfterLogin();

                dynamic json = JsonConvert.DeserializeObject(data);

                LineDATA linedata;

                foreach (var item in json.rows)
                {
                    if (MsgRefNumber == convert.ToString(item.enquireNo))
                    {
                        string enquireMainId = item.enquireMainId;
                        string status = item.status;
                        string enquireNo = item.enquireNo;


                        string data1 = GetDATA(enquireMainId, status, enquireNo);
                         linedata = JsonConvert.DeserializeObject<LineDATA>(data1);
                        LineItemPostDATA asdf = postBodyForLineItem(linedata);
                        string json1 = JsonConvert.SerializeObject(asdf.LineItemData);
                        string encodedString = HttpUtility.UrlEncode(json1);
                        string lineResponse = LineItemPostRequest(encodedString, enquireMainId);
                        LineResponse obj = JsonConvert.DeserializeObject<LineResponse>(lineResponse);

                        if(obj.Success == true)
                        {
                            LogText = "LineItem Saves SuccessFully";

                            string dynamicString = $"enquireMainId={enquireMainId}&enquireNo={enquireNo}&vesselCode={item.vesselCode}&orderType={item.orderType}&status={status}&principalGroupCode={item.principalGroupCode}&supplierCode={item.supplierCode}&enquireDate={item.enquireDate}&effDateTo={item.effDateTo}&quoteLimit=&sendVesselPosition={item.sendVesselPosition}&sendVesselDate=&taxRate=&currency={item.currency}&carriageFee=&checkFee=&orderFee=&purRegulaPath=&purRegulaName=&payOffAmount=&remark=&supplierRemark={EditCommentAndEncode(SupplierComment)}&updateSign=BJ";

                            string a = saveQuoteRequest(dynamicString);

                            UploadAttachmentIfPresent(linedata);

                            if(IsSubmitQuote)
                            {
                                submitQuote(status, enquireMainId);
                            }

                        }
                        

                        break;
                    }
                }



            }
            catch (Exception ex)
            {
                LogText = "Unable to process the file -" + ex.Message;
                CreateAuditFile(Path.GetFileName(MTMLFile), processorname, UCRefNo, "Error", "Unable to process the file -" + ex.Message, BuyerCode, SupplierCode, AuditPath);
                MoveFiles(MTMLFile, QuotePath + "\\Error\\" + Path.GetFileName(MTMLFile));
            }
        }


        public void submitQuote(string status,string enquireMainId)
        {
            string responce = "";
            try
            {

                string body = $"enquireMainId={enquireMainId}&status={status}&approveInd=";

                referrer = _httpWrapper._CurrentResponse.ResponseUri.OriginalString;
                _httpWrapper.Referrer = referrer;
                _httpWrapper.RequestMethod = "POST";


                _httpWrapper.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                _httpWrapper.AcceptMimeType = "*/*";
                string SubmitQuoteUrl = SAVEQUOTEURL + "?_dc=0.6703007274001826&_cpid=8a9282cd798a11e201799d776eff0207";


                bool Result = _httpWrapper.PostURL(SubmitQuoteUrl, body, "", "", "");

                if (Result)
                {


                    responce = _httpWrapper._CurrentResponseString;


                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogText = "Error in saveQuoteRequest  " + ex.Message;

                throw ex;
            }
            //return responce;
        }


        static string EditCommentAndEncode(string input)
        {
            string newString = "For full comments , please refer to the PDF Quotation " + input;


          return HttpUtility.UrlEncode(newString);
           
        }


        public LineItemPostDATA postBodyForLineItem(LineDATA data)
        {
            LogText = "Started filling post body for LineItems";


            LineItemPostDATA postDATA = new LineItemPostDATA();
            postDATA.LineItemData = new List<LineItemData>();

           
            try
            {
                for (int i=0; i < data.total; i++)
                {

                    LineItemData lineItem = new LineItemData();

                    lineItem.enquireDetailId = data.rows[i].enquireDetailId;
                    lineItem.oldEnquireDetailId = "";
                    lineItem.enquireMainId = data.rows[i].enquireMainId;
                    lineItem.enquireNo = "";
                    lineItem.applyDetailId= data.rows[i].applyDetailId;
                    lineItem.applyMainId = data.rows[i].applyMainId;
                    lineItem.dept = data.rows[i].dept;
                    lineItem.applyNo = data.rows[i].applyNo;
                    lineItem.status = data.rows[i].status;
                    lineItem.isUrgency = data.rows[i].isUrgency;
                    lineItem.applyType = data.rows[i].applyType;
                    lineItem.bizType = data.rows[i].bizType;
                    lineItem.vesselCode = data.rows[i].vesselCode;
                    lineItem.vesselName = ConvertToUnicodeEscape(data.rows[i].vesselName);
                    lineItem.deployVessel = "";
                    lineItem.groupType = data.rows[i].groupType;
                    lineItem.parentId = data.rows[i].parentId;
                    lineItem.parentNo = data.rows[i].parentNo;
                    lineItem.dataType = data.rows[i].dataType;
                    lineItem.sendVesselInd = "";
                    lineItem.sendVesselStatus = "";
                    lineItem.sendVesselStatusName = "";
                    lineItem.sendVesselNo = "";
                    lineItem.supplierNum = "";
                    lineItem.equipmentId = data.rows[i].equipmentId;
                    lineItem.equipCode = data.rows[i].equipCode;
                    lineItem.equipName = ConvertToUnicodeEscape(data.rows[i].equipName);
                    lineItem.model = data.rows[i].model;
                    lineItem.partsInfoId = data.rows[i].partsInfoId;
                    lineItem.partsNo = data.rows[i].partsNo;
                    lineItem.bookingNo = "";//data.rows[i].bookingNo;
                    lineItem.partsName = data.rows[i].partsName;//ConvertToUnicodeEscape(data.rows[i].partsName);
                    lineItem.partsNameEn = data.rows[i].partsNameEn;
                    lineItem.isFixedAsset = "";
                    lineItem.ifStoraged = "";//data.rows[i].ifStoraged;
                    lineItem.groupCode = "";//data.rows[i].groupCode;
                    lineItem.comQty = "";
                    lineItem.stockQty ="" ;
                    
                    lineItem.inquiryQty = data.rows[i].inquiryQty;
                    
                    lineItem.orderQty = "";
                    lineItem.checkAuditQty = "";
                    lineItem.checkFee = "";
                    lineItem.checkFeeCny = "";
                    lineItem.storagedQty = "";
                    lineItem.notStoragedQty = "";
                    lineItem.unUseQty = "";
                    lineItem.unit = data.rows[i].unit;
                   
                    lineItem.payOff = "";
                    lineItem.sreviceRate = "";
                    lineItem.taxRate = null;//"";
                    lineItem.taxFee = "";
                    
                    lineItem.feeWithoutTaxOff = null;
                    lineItem.exchangeRate = "";
                    lineItem.cnyPrice = "";
                    lineItem.cnyFee = "";
                    lineItem.currency = "";
                    lineItem.currencyName = "";
                    lineItem.supplierCode = data.rows[i].supplierCode;
                    lineItem.supplierName = ConvertToUnicodeEscape(data.rows[i].supplierName);
                    lineItem.mainRemark = data.rows[i].mainRemark;
                    lineItem.applyUserName = ConvertToUnicodeEscape(data.rows[i].applyUserName);
                    lineItem.applyUserTel = data.rows[i].applyUserTel;
                    lineItem.remark = data.rows[i].remark; //ConvertToUnicodeEscape(data.rows[i].remark);


                    lineItem.remarkFlag = data.rows[i].remarkFlag;
                    lineItem.cusRemarkFlag = "";
                    lineItem.isDelete = data.rows[i].isDelete;
                    lineItem.cancelReason = "";
                    lineItem.isAttact = "";
                    lineItem.minPriceSeq = "";
                    lineItem.checkQuotation = "";
                    lineItem.deployQty = "";
                    lineItem.deployStockQty = "";
                    lineItem.importCondition = "";
                    lineItem.placeOfOrigin = "";
                    lineItem.oilFee = "";
                    lineItem.carriageFee = "";
                    lineItem.brandQuality = "N/A";
                    lineItem.seqNo = data.rows[i].seqNo;
                    lineItem.orderDate = null;
                    lineItem.applyDate = data.rows[i].applyDate;
                    lineItem.sendVesselDate = null;


                    for(int j = 0;j< _QuoteLineItems.Count;j++)
                    {
                        if (_QuoteLineItems[j].UDF1 == data.rows[i].partsInfoId)
                        {
                            lineItem.supplierRemark = _QuoteLineItems[j].LineItemComment.Value;
                            lineItem.feeWithoutTax = _QuoteLineItems[j].BuyerItemTotal.ToString();
                            lineItem.fee = _QuoteLineItems[j].BuyerItemTotal.ToString();
                            lineItem.price = _QuoteLineItems[j].PriceList[0].Value;
                            lineItem.checkPrice = _QuoteLineItems[j].PriceList[0].Value;
                            lineItem.getMatLastDay = _QuoteLineItems[j].DeleiveryTime;
                            lineItem.applyQty = (int)_QuoteLineItems[j].Quantity;
                            lineItem.auditQty = (int)_QuoteLineItems[j].Quantity;
                            lineItem.checkQty = (int)_QuoteLineItems[j].Quantity;


                        }
                    }



                    postDATA.LineItemData.Add(lineItem);


                }
                LogText = "Completed post body for LineItems";

                return postDATA;

            }
            catch (Exception ex)
            {
                LogText = "Issue in creating  post body for LineItems" + ex.Message;

            }
            return postDATA;
        }


        public static string ConvertToUnicodeEscape(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (c > 127) // Check if character is non-ASCII
                {
                    sb.AppendFormat("\\u{0:X4}", (int)c); // Convert to Unicode escape sequence
                }
                else
                {
                    sb.Append(c); // Append ASCII characters as they are
                }
            }
            return sb.ToString();
        }


        private string LineItemPostRequest(string encodedString, string enquireMainId)
        {
            
            string responce = "";
            try
            {

                string body = $"batch={encodedString}&mainId={enquireMainId}";

                referrer = _httpWrapper._CurrentResponse.ResponseUri.OriginalString;
                _httpWrapper.Referrer = referrer;
                _httpWrapper.RequestMethod = "POST";

                //_httpWrapper._AddRequestHeaders.Add("origin", origin);
                _httpWrapper.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                _httpWrapper.AcceptMimeType = "*/*";
                string LineItemPostUrl = LINEITEMPOSTURL;


                bool Result = _httpWrapper.PostURL(LineItemPostUrl, body, "", "", "");

                if (Result)
                {
                    
                     responce = _httpWrapper._CurrentResponseString;
                   
                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {
                LogText = "Error in LineItemPostRequest  " +ex.Message;
                 throw ex;
            }
            return responce;
        }


        private string saveQuoteRequest(string dynamicString)
        {
            string responce = "";
            try
            {

                string body = dynamicString;

                referrer = _httpWrapper._CurrentResponse.ResponseUri.OriginalString;
                _httpWrapper.Referrer = referrer;
                _httpWrapper.RequestMethod = "POST";

               
                _httpWrapper.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                _httpWrapper.AcceptMimeType = "*/*";
                string saveQuoteUrl = SAVEQUOTEURL;


                bool Result = _httpWrapper.PostURL(saveQuoteUrl, body, "", "", "");

                if (Result)
                {
                   

                    responce = _httpWrapper._CurrentResponseString;

                    
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogText = "Error in saveQuoteRequest  " + ex.Message;

                throw ex;
            }
            return responce;
        }

        private void UploadAttachmentIfPresent(LineDATA linedata)
        {
            try
            {
                //LogText = " processing Started";
                DirectoryInfo _dir = new DirectoryInfo(cEmbdPdfAttachPath);
                FileInfo[] _Files = _dir.GetFiles();
                if (_Files.Length > 0)
                {
                    foreach (FileInfo _MtmlFile in _Files)
                    {
                        if (_MtmlFile.Name.Contains(AAGRefNo))
                        {
                            //LogText = _dir.FullName +"\\"+ _MtmlFile;

                           

                            bool attach = UploadPdfFile(_MtmlFile, linedata);

                            if (attach)
                            {
                                //SaveQuoteFile(quote, webResponse, _MtmlFile);
                                LogText = "Quote Attachment : " + _MtmlFile + " uploaded sucessfully";
                                CreateAuditFile(Path.GetFileName(_MtmlFile.ToString()), processorname, UCRefNo, "Success", "Attachment Uploaded Successfully.", BuyerCode, SupplierCode, AuditPath);
                                MoveFiles(_MtmlFile.FullName, cEmbdPdfAttachPath + "\\Backup\\" + Path.GetFileName(_MtmlFile.FullName));
                            }
                        }
                    }
                }
                else
                {
                    LogText = "No Quote files found to process Quote Attachment";
                }
                //LogText = "Quote processing Stopped.";
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        public bool UploadPdfFile(FileInfo File,LineDATA linedata)
        {
            bool result = false;
            HttpWebResponse response = null;
            //string boundary = GenerateBoundary();
            string boundary = DateTime.Now.Ticks.ToString("x", System.Globalization.NumberFormatInfo.InvariantInfo);
            string contentType = "multipart/form-data; boundary=" + boundary;
            string uuid = linedata.rows[0].parentId;// GenerateUUID();
            string postUrl = UPLOADFILEURL;
            string abc = GetCookieDetails();
            

            try
            {
                


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(postUrl);

                webRequest.KeepAlive = true;
                webRequest.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                webRequest.Headers.Set("Upgrade-Insecure-Requests", @"1");
                webRequest.Headers.Add("Origin", origin);

                webRequest.ContentType = contentType;
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
                webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
                webRequest.Referer = "http://htsm.fjhighton.com/main.jsp";
                webRequest.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                webRequest.Headers.Set(HttpRequestHeader.Cookie, @"ssmp_lang=CN; ssmp_uid=13918965973; ssmp_saveUid=1; ssmp_savePwd=1; ssmp_pwd=gys@965973; " +abc + "tipServerRestarted=0");

                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;



                string[] bodyParts = new string[]{$"--{boundary}\r\n", "Content-Disposition: form-data; name=\"bisiMainId\"\r\n\r\n", $"{uuid}\r\n",$"--{boundary}\r\n", "Content-Disposition: form-data; name=\"attachSpeType\"\r\n\r\n", "PUR\r\n",$"--{boundary}\r\n", "Content-Disposition: form-data; name=\"supplierCode\"\r\n\r\n", $"{linedata.rows[0].supplierCode}\r\n",$"--{boundary}\r\n", "Content-Disposition: form-data; name=\"maxFileSize\"\r\n\r\n", "10\r\n",$"--{boundary}\r\n",$"Content-Disposition: form-data; name=\"file\"; filename=\"{File.Name}\"\r\n","Content-Type: application/pdf\r\n\r\n",$"{File.FullName}\r\n",$"--{boundary}--"};

                WriteMultipartBodyToRequest(webRequest, boundary, string.Concat(bodyParts));

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webResponse != null)
                {
                    string ab = _httpWrapper.ReadResponse(webResponse);
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(ab);
                    if(data.success == true)
                    {
                        result= true;
                    }
                }
            }
            catch (WebException ex)
            {
                LogText = "Unable to upload Attachment " + ex.Message;

                return result;
            }
           

            return result;
        }

        private string GetCookieDetails()
        {
            string cResult = "";
            if (_httpWrapper._dctSetCookie.Count > 0)
            {
                foreach (string key in _httpWrapper._dctSetCookie.Keys)
                {
                    cResult += key + "=" + _httpWrapper._dctSetCookie[key] + "; ";
                }
            }
            return cResult.TrimEnd(';');
        }

        public string GenerateBoundary()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var boundary = new char[16];
            for (int i = 0; i < boundary.Length; i++)
            {
                boundary[i] = chars[random.Next(chars.Length)];
            }
            return "----WebKitFormBoundary" + new string(boundary);
        }

        public string GenerateUUID()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }


        public void WriteMultipartBodyToRequest(HttpWebRequest request, string boundary, string body)
        {
            string[] multiparts = Regex.Split(body, @"<!>");
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (string part in multiparts)
                {
                    if (File.Exists(part))
                    {
                        bytes = File.ReadAllBytes(part);
                    }
                    else
                    {
                        bytes = Encoding.UTF8.GetBytes(part.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"));
                    }

                    ms.Write(bytes, 0, bytes.Length);
                }

                request.ContentLength = ms.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    ms.WriteTo(stream);
                }
            }
        }

    }
}
