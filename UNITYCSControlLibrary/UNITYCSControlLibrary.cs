using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNITYCSControlLibrary
{
    public class UNITYCSDataFormatting
    {
        public String Hyphon(String strIn)
        {
            String strOut = "";
            int i;

            if (strIn != null)
            {
                for (i = 0; i <= strIn.Length - 1; i++)
                {
                    if (strIn.Substring(i, 1) == "'")
                        strOut = strOut + "''";
                    else
                        strOut = strOut + strIn.Substring(i, 1);
                }
            }

            return strOut;
        }
        public String SymbolStrip(String strInput, String strSymbols)
        {
            int i;
            int j;
            String strOutput = "";
            String c;

            for (i = 0; i < strSymbols.Length; i++)
            {
                strOutput = "";
                c = strSymbols.Substring(i, 1);

                for (j = 0; j < strInput.Length; j++)
                {
                    if (c != strInput.Substring(j, 1))
                        strOutput = strOutput + strInput.Substring(j, 1);
                }
                strInput = strOutput;

            }

            return strOutput;
        }
    }
    public class UNITYCSDataValidation
    {

    }
    public class UNITYCSControlLibrary
    {
        public SqlConnection myConnection { get; set; } = new SqlConnection();
        public String errorMessage { get; set; } = string.Empty;
        private UNITYCSDataFormatting myFormatting = new UNITYCSDataFormatting();

        #region GST Code Table
        public DataTable GSTCodeList { get; set; } = new DataTable();
        public Int32 GSTCodeId { get; set; }
        public String GSTCode { get; set; }
        public String GSTCodeDescription { get; set; }
        public Double GSTCodeRate { get; set; }

        private DataTable thisGSTCode = new DataTable();

        public Boolean Insert_GST_Code(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblGSTCodes (";
                strSQL = strSQL + "gst_code, ";
                strSQL = strSQL + "gst_description, ";
                strSQL = strSQL + "gst_rate) VALUES (";
                strSQL = strSQL + "'" + GSTCode + "', ";
                strSQL = strSQL + "'" + myFormatting.Hyphon(GSTCodeDescription) + "', ";
                strSQL = strSQL + GSTCodeRate.ToString() + ")";
                SqlCommand cmdInsert = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nInsert GST Code:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nInsert GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_GST_Code(String gstCode)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;
            thisGSTCode.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblGSTCodes WHERE gst_code = '" + gstCode + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisGSTCode.Load(rdrGet);
                    isSuccessful = Gather_GST_Code();
                }
                else
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nGet GST Code:\r\n\r\nGST Code " + gstCode + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_GST_Code()
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                GSTCodeId = Convert.ToInt32(thisGSTCode.Rows[0]["gst_id"]);
                GSTCode = thisGSTCode.Rows[0]["gst_code"].ToString();
                GSTCodeDescription = thisGSTCode.Rows[0]["gst_description"].ToString();
                GSTCodeRate = Convert.ToDouble(thisGSTCode.Rows[0]["gst_rate"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGather GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_GST_Code(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblGSTCodes SET ";
                if (GSTCodeDescription !=  thisGSTCode.Rows[0]["gst_description"].ToString())
                {
                    strSQL = strSQL + "gst_description = '" + myFormatting.Hyphon(GSTCodeDescription) + "', ";
                    hasChanged = true;
                }
                if (GSTCodeRate != Convert.ToDouble(thisGSTCode.Rows[0]["gst_rate"]))
                {
                    strSQL = strSQL + "gst_rate = " + GSTCodeRate.ToString() + ", ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2) + " WHERE gst_id = " + GSTCodeId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, myConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        errorMessage = "** Operator **\r\n\r\nUpdate GST Code:\r\n\r\nMore than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nUpdate GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_GST_Code(Int32 gstCodeId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblGSTCodes WHERE gst_id = " + gstCodeId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nDelete GST Code:\r\n\r\nMore than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nDelete GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_GST_Code_List()
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;
            GSTCodeList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblGSTCodes ORDER BY gst_code";
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    GSTCodeList.Load(rdrGet);
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet GST Code List:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Charges Master Table

        public DataTable ChargesList { get; set; } = new DataTable();
        public Int32 ChargeId { get; set; }
        public String ChargeDescription { get; set; }
        public String ChargeGSTCode { get; set; }

        private DataTable thisCharge = new DataTable();

        public Boolean Insert_Charge(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblCharges (";
                strSQL = strSQL + "charge_description, ";
                strSQL = strSQL + "charge_gstcode) VALUES (";
                strSQL = strSQL + "'" + myFormatting.Hyphon(ChargeDescription) + "', ";
                strSQL = strSQL + "'" + ChargeGSTCode + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nInsert New Charges:\r\n\r\nMore than one record would be Inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nInsert New Charges:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Charge(Int32 chargeId)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblCharges WHERE charge_id = " + chargeId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisCharge.Clear();
                    thisCharge.Load(rdrGet);
                    isSuccessful = Gather_Charge();
                }
                else
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\nCharge with Id " + chargeId.ToString() + " not found !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Charge(Int32 chargeId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblCharges WHERE charge_id = " + chargeId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisCharge.Clear();
                    thisCharge.Load(rdrGet);
                    isSuccessful = Gather_Charge();
                }
                else
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\nCharge with Id " + chargeId.ToString() + " not found !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Charge()
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                ChargeId = Convert.ToInt32(thisCharge.Rows[0]["charge_id"]);
                ChargeDescription = thisCharge.Rows[0]["charge_description"].ToString();
                ChargeGSTCode = thisCharge.Rows[0]["charge_gstcode"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGather Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Charge(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblCharges SET ";
                if (ChargeDescription != thisCharge.Rows[0]["charge_description"].ToString())
                {
                    strSQL = strSQL + "charge_description = '" + myFormatting.Hyphon(ChargeDescription) + "', ";
                    hasChanged = true;
                }
                if (ChargeGSTCode != thisCharge.Rows[0]["charge_gstcode"].ToString())
                {
                    strSQL = strSQL + "charge_gstcode = '" + ChargeGSTCode + "', ";
                    hasChanged = true;
                }
                 
                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Trim().Length - 1) + " WHERE charge_id = " + ChargeId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, myConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        errorMessage = "** Operator **\r\n\r\nUpdate Charge:\r\n\r\nMore than one Record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nUpdate Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Charge(Int32 chargeId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblCharges WHERE charge_id = " + chargeId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nDelete Charge:\r\n\r\nMore than one record would be Deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nDelete Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Phrases Table
        public DataTable PhrasesList { get; set; } = new DataTable();
        public Int32 PhraseId { get; set; }
        public String PhraseDescription { get; set; }

        private DataTable thisPhrase = new DataTable();

        public Boolean Insert_Phrase(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblPhrases (phrase_description) VALUES ('" + myFormatting.Hyphon(PhraseDescription) + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nInsert New Phrase:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nInsert New Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Phrase(Int32 phraseId)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;
            thisPhrase.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblPhrases WHERE phrase_id = " + phraseId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisPhrase.Load(rdrGet);
                    isSuccessful = Gather_Phrase();
                }
                else
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nGet Phrase:\r\n\r\nPhrase not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Phrase()
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                PhraseId = Convert.ToInt32(thisPhrase.Rows[0]["phrase_id"]);
                PhraseDescription = thisPhrase.Rows[0]["phrase_description"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGather Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Phrase(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblPhrases SET ";
                if (PhraseDescription != thisPhrase.Rows[0]["phrase_description"].ToString())
                {
                    strSQL = strSQL + "phrase_description = '" + myFormatting.Hyphon(PhraseDescription) + "' ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL + "WHERE phrase_id = " + PhraseId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, myConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        errorMessage = "** Operator **\r\n\r\nUpdate Phrase:\r\n\r\nMore than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nUpdate Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Phrase(Int32 phraseId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblPhrases WHERE phrase_id = " + phraseId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nDelete Phrase:\r\n\r\nMore than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nDelete Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_List_Of_Phrases()
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;
            PhrasesList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblPhrases ORDER BY phrase_description";
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    PhrasesList.Load(rdrGet);
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet List Of Phrases:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Sales Table
        public DataTable SalesList { get; set; } = new DataTable();
        public Int32 SaleId { get; set; }
        public String SaleDescription { get; set; }
        public DateTime SaleDate { get; set; }
        public Boolean SaleIsActive { get; set; }
        public String SaleDataSource { get; set; }

        private DataTable thisSale { get; set; } = new DataTable();

        public Boolean Insert_Sale(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblSales (";
                strSQL = strSQL + "sales_description, ";
                strSQL = strSQL + "sales_date, ";
                strSQL = strSQL + "sales_active, ";
                strSQL = strSQL + "sales_datasource) VALUES (";
                strSQL = strSQL + "'" + myFormatting.Hyphon(SaleDescription) + "', ";
                strSQL = strSQL + "CONVERT(datetime, '" + SaleDate.ToString() + "', 103) ";
                strSQL = strSQL + "'" + SaleIsActive.ToString() + "', ";
                strSQL = strSQL + "'" + myFormatting.Hyphon(SaleDataSource) + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Sale(Int32 saleId)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;
            thisSale.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblSales WHERE sales_id = " + saleId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisSale.Load(rdrGet);
                    isSuccessful = Gather_Sale();
                }
                else
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nGet Sale:\r\n\r\nSale Id not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGet Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Sale()
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                SaleId = Convert.ToInt32(thisSale.Rows[0]["sales_id"]);
                SaleDescription = thisSale.Rows[0]["sales_description"].ToString();
                SaleDate = Convert.ToDateTime(thisSale.Rows[0]["sales_date"]);
                SaleIsActive = Convert.ToBoolean(thisSale.Rows[0]["sales_active"]);
                SaleDataSource = thisSale.Rows[0]["sales_datasource"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nGather Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Sale(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblSales SET ";
                if (SaleDescription != thisSale.Rows[0]["sales_description"].ToString())
                {
                    strSQL = strSQL + "sales_description = '" + myFormatting.Hyphon(SaleDescription) + "', ";
                    hasChanged = true;
                }
                if (SaleDate != Convert.ToDateTime(thisSale.Rows[0]["sales_date"]))
                {
                    strSQL = strSQL + "sales_date =  CONVERT(datetime, '" + SaleDate.ToString() + "', 103), ";
                    hasChanged = true;
                }
                if (SaleIsActive != Convert.ToBoolean(thisSale.Rows[0]["sales_active"]))
                {
                    strSQL = strSQL + "sales_active = '" + SaleIsActive.ToString() + "', ";
                    hasChanged = true;
                }
                if (SaleDataSource != thisSale.Rows[0]["sales_datasource"].ToString())
                {
                    strSQL = strSQL + "sales_datasource = '" + myFormatting.Hyphon(SaleDataSource) + "', ";
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2) + " WHERE sales_id = " + SaleId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, myConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        errorMessage = "** Operator **\r\n\r\nUpdate Sale:\r\n\r\nMore than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nUpdate Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Sale(Int32 saleId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblSales WHERE sales_id = " + saleId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, myConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nDelete Sale:\r\n\r\nMore than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nDelete Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_List_Of_Sales(String activeFilter)
        {
            Boolean isSuccessful = true;

            errorMessage = string.Empty;
            SalesList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblSales ";
                if (activeFilter == "Active")
                    strSQL = strSQL + "WHERE sales_active = '" + true.ToString() + "' ";
                else if (activeFilter == "Inactive")
                    strSQL = strSQL + "WHERE sales_active = '" + false.ToString() + "' ";
                strSQL = strSQL + "ORDER BY sales_date DESC";
                SqlCommand cmdGet = new SqlCommand(strSQL, myConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    SalesList.Load(rdrGet);
                }
                else
                {
                    isSuccessful = false;
                    errorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\nNo records found !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                errorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
    }
    public class UNITYCSSaleLibrary
    {
        public SqlConnection myConnection { get; set; } = new SqlConnection();
        public String errorMessage { get; set; } = string.Empty;
        private UNITYCSDataFormatting myFormatting = new UNITYCSDataFormatting();

    }
}
