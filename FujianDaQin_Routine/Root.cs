using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FujianDaQin_Routine
{
    public class Root
    {
        public int total { get; set; }
        public List<Row> rows { get; set; }
    }

    public class Row
    {
        public string applyBusiType { get; set; }
        public string applyDate { get; set; }
        public string applyDetailId { get; set; }
        public string applyMainId { get; set; }
        public string applyNo { get; set; }
        public int applyQty { get; set; }
        public string applyType { get; set; }
        public string applyUser { get; set; }
        public string applyUserName { get; set; }
        public string applyUserTel { get; set; }
        public int auditQty { get; set; }
        public string bizType { get; set; }
        public string bookingNo { get; set; }
        public string brandQuality { get; set; }
        public int checkAuditQty { get; set; }
        public double checkPrice { get; set; }
        public int checkQty { get; set; }
        public bool checkQuotation { get; set; }
        public int cnyFee { get; set; }
        public int cnyPrice { get; set; }
        public string companyCode { get; set; }
        public string createdByUser { get; set; }
        public string createdDtmLoc { get; set; }
        public string createdOffice { get; set; }
        public string currency { get; set; }
        public string currencyName { get; set; }
        public string dataType { get; set; }
        public string dept { get; set; }
        public string enquireDetailId { get; set; }
        public string enquireMainId { get; set; }
        public string equipCode { get; set; }
        public string equipName { get; set; }
        public string equipmentId { get; set; }
        public int fee { get; set; }
        public int feeWithoutTax { get; set; }
        public int feeWithoutTaxOff { get; set; }
        public string getMatLastDay { get; set; }
        public string groupType { get; set; }
        public string ifStoraged { get; set; }
        public int inquiryQty { get; set; }
        public string isDelete { get; set; }
        public string isUrgency { get; set; }
        public string mainRemark { get; set; }
        public int maxSafetyStock { get; set; }
        public int minSafeStock { get; set; }
        public string model { get; set; }
        public string newDrawingNo { get; set; }
        public int orderQty { get; set; }
        public string parentId { get; set; }
        public string parentNo { get; set; }
        public string partsInfoId { get; set; }
        public string partsName { get; set; }
        public string partsNameEn { get; set; }
        public string partsNo { get; set; }
        public string placeOfOrigin { get; set; }
        public double price { get; set; }
        public string principalGroupCode { get; set; }
        public int recordVersion { get; set; }
        public string remark { get; set; }
        public string remarkFlag { get; set; }
        public string sendVesselInd { get; set; }
        public string sendVesselNo { get; set; }
        public string sendVesselStatus { get; set; }
        public int seqNo { get; set; }
        public string status { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string supplierRemark { get; set; }
        public int taxFee { get; set; }
        public string unit { get; set; }
        public string unitName { get; set; }
        public string updatedByUser { get; set; }
        public string updatedDtmLoc { get; set; }
        public string updatedTimeZone { get; set; }
        public string vesselCode { get; set; }
        public string vesselName { get; set; }
    }

}
