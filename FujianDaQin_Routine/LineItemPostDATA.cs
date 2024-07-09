using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FujianDaQin_Routine
{
    public class LineItemPostDATA
    {

        public List<LineItemData> LineItemData {  get; set; }
       
    }

    public class LineItemData
    {
         public string enquireDetailId { get; set; }
        public string oldEnquireDetailId { get; set; }
        public string enquireMainId { get; set; }
        public string enquireNo { get; set; }
        public string applyDetailId { get; set; }
        public string applyMainId { get; set; }
        public string dept { get; set; }
        public string applyNo { get; set; }
        public string status { get; set; }
        public string isUrgency { get; set; }
        public string applyType { get; set; }
        public string bizType { get; set; }
        public string vesselCode { get; set; }
        public string vesselName { get; set; }
        public string deployVessel { get; set; }
        public string groupType { get; set; }
        public string parentId { get; set; }
        public string parentNo { get; set; }
        public string dataType { get; set; }
        public string sendVesselInd { get; set; }
        public string sendVesselStatus { get; set; }
        public string sendVesselStatusName { get; set; }
        public string sendVesselNo { get; set; }
        public string supplierNum { get; set; }
        public string equipmentId { get; set; }
        public string equipCode { get; set; }
        public string equipName { get; set; }
        public string model { get; set; }
        public string partsInfoId { get; set; }
        public string partsNo { get; set; }
        public string bookingNo { get; set; }
        public string partsName { get; set; }
        public string partsNameEn { get; set; }
        public string isFixedAsset { get; set; }
        public string ifStoraged { get; set; }
        public string groupCode { get; set; }
        public string comQty { get; set; }
        public string stockQty { get; set; }
        public int applyQty { get; set; }
        public int auditQty { get; set; }
        public int inquiryQty { get; set; }
        public int checkQty { get; set; }
        public string orderQty { get; set; }
        public string checkAuditQty { get; set; }
        public string checkFee { get; set; }
        public string checkFeeCny { get; set; }
        public string storagedQty { get; set; }
        public string notStoragedQty { get; set; }
        public string unUseQty { get; set; }
        public string unit { get; set; }
        public double price { get; set; }
        public double checkPrice { get; set; }
        public string payOff { get; set; }
        public string sreviceRate { get; set; }
        public object taxRate { get; set; }
        public string fee { get; set; }
        public string taxFee { get; set; }
        public string feeWithoutTax { get; set; }
        public string feeWithoutTaxOff { get; set; }
        public string exchangeRate { get; set; }
        public string cnyPrice { get; set; }
        public string cnyFee { get; set; }
        public string currency { get; set; }
        public string currencyName { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string mainRemark { get; set; }
        public string applyUserName { get; set; }
        public string applyUserTel { get; set; }
        public string remark { get; set; }
        public string supplierRemark { get; set; }
        public string remarkFlag { get; set; }
        public string cusRemarkFlag { get; set; }
        public string isDelete { get; set; }
        public string cancelReason { get; set; }
        public string isAttact { get; set; }
        public string minPriceSeq { get; set; }
        public string checkQuotation { get; set; }
        public string deployQty { get; set; }
        public string getMatLastDay { get; set; }
        public string deployStockQty { get; set; }
        public string importCondition { get; set; }
        public string placeOfOrigin { get; set; }
        public string oilFee { get; set; }
        public string carriageFee { get; set; }
        public string brandQuality { get; set; }
        public int seqNo { get; set; }
        public DateTime? orderDate { get; set; }
        public DateTime applyDate { get; set; }
        public DateTime? sendVesselDate { get; set; }
    }
}
