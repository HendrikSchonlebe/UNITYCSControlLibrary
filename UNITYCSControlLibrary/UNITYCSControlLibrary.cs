﻿using System;
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
        const int ABN_Length = 11;
        const int ABN_TOO_SHORT = 1;
        const int ABN_TOO_LONG = 2;
        const int ABN_NOT_NUMERIC = 3;
        const int ABN_INVALID = 4;

        public String ErrorMessage { get; set; } = string.Empty;

        public Boolean IsEmailAddress(String strInput)
        {
            Boolean logValid = true;

            ErrorMessage = string.Empty;

            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

            try
            {
                logValid = rgx.Match(strInput).Success;
            }
            catch (Exception ex)
            {
                ErrorMessage = "** Operator **\r\n\r\nData Validation - IsEmailAddress " + ex.Message + " !";
            }

            return logValid;
        }
        public Boolean IsValid_ABN(String testABN)
        {
            Boolean isValid = true;

            String[] ERROR_MESSAGES = { "", "ABN must contain at least " + ABN_Length.ToString() + " digits", "ABN cannot be more than " + ABN_Length.ToString() + " digits", "ABN must be numeric", "ABN submitted is invalid" };
            int _errorCode = 0;
            int[] digitWeight = { 10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
            int testNumber;
            int testResult;

            // Remove any spaces from the string 
            testABN = System.Text.RegularExpressions.Regex.Replace(testABN, @"\s+", "");

            if (testABN.Trim().Length == ABN_Length)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(testABN, @"\b\d+\b", System.Text.RegularExpressions.RegexOptions.None) == true)
                {
                    int[] digitValue = { Convert.ToInt16(testABN.Substring(0,1)),
                                             Convert.ToInt16(testABN.Substring(1,1)),
                                             Convert.ToInt16(testABN.Substring(2,1)),
                                             Convert.ToInt16(testABN.Substring(3,1)),
                                             Convert.ToInt16(testABN.Substring(4,1)),
                                             Convert.ToInt16(testABN.Substring(5,1)),
                                             Convert.ToInt16(testABN.Substring(6,1)),
                                             Convert.ToInt16(testABN.Substring(7,1)),
                                             Convert.ToInt16(testABN.Substring(8,1)),
                                             Convert.ToInt16(testABN.Substring(9,1)),
                                             Convert.ToInt16(testABN.Substring(10,1))
                                           };
                    digitValue[0] = digitValue[0] - 1;
                    testNumber = (digitValue[0] * digitWeight[0]) +
                                 (digitValue[1] * digitWeight[1]) +
                                 (digitValue[2] * digitWeight[2]) +
                                 (digitValue[3] * digitWeight[3]) +
                                 (digitValue[4] * digitWeight[4]) +
                                 (digitValue[5] * digitWeight[5]) +
                                 (digitValue[6] * digitWeight[6]) +
                                 (digitValue[7] * digitWeight[7]) +
                                 (digitValue[8] * digitWeight[8]) +
                                 (digitValue[9] * digitWeight[9]) +
                                 (digitValue[10] * digitWeight[10]);

                    testResult = testNumber / 89;

                    if ((testResult * 89) != testNumber)
                    {
                        isValid = false;
                        _errorCode = ABN_INVALID;
                    }
                }
                else
                {
                    isValid = false;
                    _errorCode = ABN_NOT_NUMERIC;
                }
            }
            else
            {
                isValid = false;
                if (testABN.Trim().Length < ABN_Length)
                    _errorCode = ABN_TOO_SHORT;
                else
                    _errorCode = ABN_TOO_LONG;
            }

            if (isValid == false)
                ErrorMessage = "** Operator **\r\n\r\n" + ERROR_MESSAGES[_errorCode] + " !";

            return isValid;
        }

    }
    public class UNITYCSControlLibrary
    {
        public SqlConnection MyConnection { get; set; } = new SqlConnection();
        public String ErrorMessage { get; set; } = string.Empty;

        private UNITYCSDataFormatting MyFormatting = new UNITYCSDataFormatting();

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

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblGSTCodes (";
                strSQL = strSQL + "gst_code, ";
                strSQL = strSQL + "gst_description, ";
                strSQL = strSQL + "gst_rate) VALUES (";
                strSQL = strSQL + "'" + GSTCode + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(GSTCodeDescription) + "', ";
                strSQL = strSQL + GSTCodeRate.ToString() + ")";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert GST Code:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_GST_Code(String gstCode)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            thisGSTCode.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblGSTCodes WHERE gst_code = '" + gstCode + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisGSTCode.Load(rdrGet);
                    isSuccessful = Gather_GST_Code();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet GST Code:\r\n\r\nGST Code " + gstCode + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_GST_Code()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

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
                ErrorMessage = "** Operator **\r\n\r\nGather GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_GST_Code(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblGSTCodes SET ";
                if (GSTCodeDescription !=  thisGSTCode.Rows[0]["gst_description"].ToString())
                {
                    strSQL = strSQL + "gst_description = '" + MyFormatting.Hyphon(GSTCodeDescription) + "', ";
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
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate GST Code:\r\n\r\nMore than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_GST_Code(Int32 gstCodeId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblGSTCodes WHERE gst_id = " + gstCodeId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete GST Code:\r\n\r\nMore than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete GST Code:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_GST_Code_List()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            GSTCodeList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblGSTCodes ORDER BY gst_code";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
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
                ErrorMessage = "** Operator **\r\n\r\nGet GST Code List:\r\n\r\n" + ex.Message + " !";
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

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblCharges (";
                strSQL = strSQL + "charge_description, ";
                strSQL = strSQL + "charge_gstcode) VALUES (";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(ChargeDescription) + "', ";
                strSQL = strSQL + "'" + ChargeGSTCode + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Charges:\r\n\r\nMore than one record would be Inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert New Charges:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Charge(Int32 chargeId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblCharges WHERE charge_id = " + chargeId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\nCharge with Id " + chargeId.ToString() + " not found !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Charge(Int32 chargeId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblCharges WHERE charge_id = " + chargeId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\nCharge with Id " + chargeId.ToString() + " not found !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Charge()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                ChargeId = Convert.ToInt32(thisCharge.Rows[0]["charge_id"]);
                ChargeDescription = thisCharge.Rows[0]["charge_description"].ToString();
                ChargeGSTCode = thisCharge.Rows[0]["charge_gstcode"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Charge(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblCharges SET ";
                if (ChargeDescription != thisCharge.Rows[0]["charge_description"].ToString())
                {
                    strSQL = strSQL + "charge_description = '" + MyFormatting.Hyphon(ChargeDescription) + "', ";
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
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Charge:\r\n\r\nMore than one Record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Charge(Int32 chargeId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblCharges WHERE charge_id = " + chargeId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Charge:\r\n\r\nMore than one record would be Deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Charge:\r\n\r\n" + ex.Message + " !";
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

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblPhrases (phrase_description) VALUES ('" + MyFormatting.Hyphon(PhraseDescription) + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Phrase:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert New Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Phrase(Int32 phraseId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            thisPhrase.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblPhrases WHERE phrase_id = " + phraseId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisPhrase.Load(rdrGet);
                    isSuccessful = Gather_Phrase();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Phrase:\r\n\r\nPhrase not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Phrase()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                PhraseId = Convert.ToInt32(thisPhrase.Rows[0]["phrase_id"]);
                PhraseDescription = thisPhrase.Rows[0]["phrase_description"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Phrase(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblPhrases SET ";
                if (PhraseDescription != thisPhrase.Rows[0]["phrase_description"].ToString())
                {
                    strSQL = strSQL + "phrase_description = '" + MyFormatting.Hyphon(PhraseDescription) + "' ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL + "WHERE phrase_id = " + PhraseId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Phrase:\r\n\r\nMore than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Phrase(Int32 phraseId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblPhrases WHERE phrase_id = " + phraseId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Phrase:\r\n\r\nMore than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Phrase:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_List_Of_Phrases()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            PhrasesList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblPhrases ORDER BY phrase_description";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
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
                ErrorMessage = "** Operator **\r\n\r\nGet List Of Phrases:\r\n\r\n" + ex.Message + " !";
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

        private DataTable ThisSale { get; set; } = new DataTable();

        public Boolean Insert_Sale(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblSales (";
                strSQL = strSQL + "sales_description, ";
                strSQL = strSQL + "sales_date, ";
                strSQL = strSQL + "sales_active, ";
                strSQL = strSQL + "sales_datasource) VALUES (";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(SaleDescription) + "', ";
                strSQL = strSQL + "CONVERT(datetime, '" + SaleDate.ToString() + "', 103) ";
                strSQL = strSQL + "'" + SaleIsActive.ToString() + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(SaleDataSource) + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Sale(Int32 saleId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisSale.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblSales WHERE sales_id = " + saleId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisSale.Load(rdrGet);
                    isSuccessful = Gather_Sale();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Sale:\r\n\r\nSale Id not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Sale()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                SaleId = Convert.ToInt32(ThisSale.Rows[0]["sales_id"]);
                SaleDescription = ThisSale.Rows[0]["sales_description"].ToString();
                SaleDate = Convert.ToDateTime(ThisSale.Rows[0]["sales_date"]);
                SaleIsActive = Convert.ToBoolean(ThisSale.Rows[0]["sales_active"]);
                SaleDataSource = ThisSale.Rows[0]["sales_datasource"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Sale(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblSales SET ";
                if (SaleDescription != ThisSale.Rows[0]["sales_description"].ToString())
                {
                    strSQL = strSQL + "sales_description = '" + MyFormatting.Hyphon(SaleDescription) + "', ";
                    hasChanged = true;
                }
                if (SaleDate != Convert.ToDateTime(ThisSale.Rows[0]["sales_date"]))
                {
                    strSQL = strSQL + "sales_date =  CONVERT(datetime, '" + SaleDate.ToString() + "', 103), ";
                    hasChanged = true;
                }
                if (SaleIsActive != Convert.ToBoolean(ThisSale.Rows[0]["sales_active"]))
                {
                    strSQL = strSQL + "sales_active = '" + SaleIsActive.ToString() + "', ";
                    hasChanged = true;
                }
                if (SaleDataSource != ThisSale.Rows[0]["sales_datasource"].ToString())
                {
                    strSQL = strSQL + "sales_datasource = '" + MyFormatting.Hyphon(SaleDataSource) + "', ";
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2) + " WHERE sales_id = " + SaleId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Sale:\r\n\r\nMore than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Sale(Int32 saleId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblSales WHERE sales_id = " + saleId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Sale:\r\n\r\nMore than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_List_Of_Sales(String activeFilter)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            SalesList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblSales ";
                if (activeFilter == "Active")
                    strSQL = strSQL + "WHERE sales_active = '" + true.ToString() + "' ";
                else if (activeFilter == "Inactive")
                    strSQL = strSQL + "WHERE sales_active = '" + false.ToString() + "' ";
                strSQL = strSQL + "ORDER BY sales_date DESC";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    SalesList.Load(rdrGet);
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\nNo records found !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Agent Details
        public String AgentName1 { get; set; }
        public String AgentName2 { get; set; }
        public String AgentStreetAddress { get; set; }
        public String AgentCityAddress { get; set; }
        public String AgentState { get; set; }
        public String AgentPostCode { get; set; }
        public String AgentMailingStreet { get; set; }
        public String AgentMailingCity { get; set; }
        public String AgentMailingState { get; set; }
        public String AgentMailingPostCode { get; set; }
        public String AgentTelephone { get; set; }
        public String AgentFax { get; set; }
        public String AgentEmail { get; set; }
        public String AgentABN { get; set; }
        public Boolean AgentGSTStatus { get; set; }
        public String AgentACN { get; set; }
        public String AgentBankName { get; set; }
        public String AgentBranchName { get; set; }
        public String AgentBSB { get; set; }
        public String AgentAccountNumber { get; set; }
        public Double AgentComissionRate1 { get; set; }
        public Double AgentComissionRate2 { get; set; }
        public Double AgentComissionRate3 { get; set; }
        public Double AgentComissionRate4 { get; set; }
        public Double AgentComissionRate5 { get; set; }
        public Boolean AgentChargePremium { get; set; }
        public Double AgentPremiumRate1 { get; set; }
        public Double AgentPremiumRate2 { get; set; }
        public Double AgentPremiumRate3 { get; set; }
        public Double AgentPremiumRate4 { get; set; }
        public Double AgentPremiumRate5 { get; set; }
        public String AgentReportsPath { get; set; }
        public String AgentInvoiceDocument { get; set; }
        public String AgentAccountSaleDocument { get; set; }
        public Boolean AgentUnityAccess { get; set; }
        public String AgentUnityDBName { get; set; }
        public String AgentUnitysaName { get; set; }
        public String AgentUnitysapwd { get; set; }

        private DataTable thisAgent = new DataTable();

        public Boolean Get_Agent_Details()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            thisAgent.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblAgent";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    thisAgent.Load(rdrGet);
                    isSuccessful = Gather_Agent_Details();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Agent's Details:\r\n\r\nAgent's Details not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Agent's Details:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        private Boolean Gather_Agent_Details()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                AgentName1 = thisAgent.Rows[0]["agent_name1"].ToString();
                AgentName2 = thisAgent.Rows[0]["agent_name2"].ToString();
                AgentStreetAddress = thisAgent.Rows[0]["agent_street"].ToString();
                AgentCityAddress = thisAgent.Rows[0]["agent_city"].ToString();
                AgentState = thisAgent.Rows[0]["agent_state"].ToString();
                AgentPostCode = thisAgent.Rows[0]["agent_postcode"].ToString();
                AgentMailingStreet = thisAgent.Rows[0]["agent_mailingstreet"].ToString();
                AgentMailingCity = thisAgent.Rows[0]["agent_mailingcity"].ToString();
                AgentMailingState = thisAgent.Rows[0]["agent_mailingstate"].ToString();
                AgentMailingPostCode = thisAgent.Rows[0]["agent_mailingpostcode"].ToString();
                AgentTelephone = thisAgent.Rows[0]["agent_telephone"].ToString();
                AgentFax = thisAgent.Rows[0]["agent_fax"].ToString();
                AgentEmail = thisAgent.Rows[0]["agent_email"].ToString();
                AgentABN = thisAgent.Rows[0]["agent_abn"].ToString();
                AgentGSTStatus = Convert.ToBoolean(thisAgent.Rows[0]["agent_gstreg"]);
                AgentACN = thisAgent.Rows[0]["agent_acn"].ToString();
                AgentBankName = thisAgent.Rows[0]["agent_bankname"].ToString();
                AgentBranchName = thisAgent.Rows[0]["agent_branch"].ToString();
                AgentBSB = thisAgent.Rows[0]["agent_bsb"].ToString();
                AgentAccountNumber = thisAgent.Rows[0]["agent_accountno"].ToString();
                AgentReportsPath = thisAgent.Rows[0]["agent_reportpath"].ToString();
                AgentInvoiceDocument = thisAgent.Rows[0]["agent_invoice"].ToString();
                AgentAccountSaleDocument = thisAgent.Rows[0]["agent_accountsale"].ToString();
                AgentUnityAccess = Convert.ToBoolean(thisAgent.Rows[0]["agent_unityaccess"]);
                AgentUnityDBName = thisAgent.Rows[0]["agent_unitydbname"].ToString();
                AgentUnitysaName = thisAgent.Rows[0]["agent_unitysaname"].ToString();
                AgentUnitysapwd = thisAgent.Rows[0]["agent_unitysapwd"].ToString();
                AgentComissionRate1 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission1"]);
                AgentComissionRate2 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission2"]);
                AgentComissionRate3 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission3"]);
                AgentComissionRate4 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission4"]);
                AgentComissionRate5 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission5"]);
                AgentChargePremium = Convert.ToBoolean(thisAgent.Rows[0]["agent_chargepremium"]);
                AgentPremiumRate1 = Convert.ToDouble(thisAgent.Rows[0]["agent_premium1"]);
                AgentPremiumRate2 = Convert.ToDouble(thisAgent.Rows[0]["agent_premium2"]);
                AgentPremiumRate3 = Convert.ToDouble(thisAgent.Rows[0]["agent_premium3"]);
                AgentPremiumRate4 = Convert.ToDouble(thisAgent.Rows[0]["agent_premium4"]);
                AgentPremiumRate5 = Convert.ToDouble(thisAgent.Rows[0]["agent_premium5"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Agent's Details:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Agent_Details(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblAgent SET ";
                if (AgentName1 != thisAgent.Rows[0]["agent_name1"].ToString())
                {
                    strSQL = strSQL + "agent_name1 = '" + MyFormatting.Hyphon(AgentName1) + "', ";
                    hasChanged = true;
                }
                if (AgentName2 != thisAgent.Rows[0]["agent_name2"].ToString())
                {
                    strSQL = strSQL + "agent_name2 = '" + MyFormatting.Hyphon(AgentName2) + "', ";
                    hasChanged = true;
                }
                if (AgentStreetAddress != thisAgent.Rows[0]["agent_street"].ToString())
                {
                    strSQL = strSQL + "agent_street = '" + MyFormatting.Hyphon(AgentStreetAddress) + "', ";
                    hasChanged = true;
                }
                if (AgentCityAddress != thisAgent.Rows[0]["agent_city"].ToString())
                {
                    strSQL = strSQL + "agent_city = '" + MyFormatting.Hyphon(AgentCityAddress) + "', ";
                    hasChanged = true;
                }
                if (AgentState != thisAgent.Rows[0]["agent_state"].ToString())
                {
                    strSQL = strSQL + "agent_state = '" + MyFormatting.Hyphon(AgentState) + "', ";
                    hasChanged = true;
                }
                if (AgentPostCode != thisAgent.Rows[0]["agent_postcode"].ToString())
                {
                    strSQL = strSQL + "agent_postcode = '" + MyFormatting.Hyphon(AgentPostCode) + "', ";
                    hasChanged = true;
                }
                if (AgentMailingStreet != thisAgent.Rows[0]["agent_mailingstreet"].ToString())
                {
                    strSQL = strSQL + "agent_mailingstreet = '" + MyFormatting.Hyphon(AgentMailingStreet) + "', ";
                    hasChanged = true;
                }
                if (AgentMailingCity != thisAgent.Rows[0]["agent_mailingcity"].ToString())
                {
                    strSQL = strSQL + "agent_mailingcity = '" + MyFormatting.Hyphon(AgentMailingCity) + "', ";
                    hasChanged = true;
                }
                if (AgentMailingState != thisAgent.Rows[0]["agent_mailingstate"].ToString())
                {
                    strSQL = strSQL + "agent_mailingstate = '" + MyFormatting.Hyphon(AgentMailingState) + "', ";
                    hasChanged = true;
                }
                if (AgentMailingPostCode != thisAgent.Rows[0]["agent_mailingpostcode"].ToString())
                {
                    strSQL = strSQL + "agent_mailingpostcode = '" + MyFormatting.Hyphon(AgentMailingPostCode) + "', ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2);
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    cmdUpdate.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Agent's Details:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
    }
    public class UNITYCSSaleLibrary
    {
        public SqlConnection MyConnection { get; set; } = new SqlConnection();
        public String ErrorMessage { get; set; } = string.Empty;
        private UNITYCSDataFormatting MyFormatting = new UNITYCSDataFormatting();

    }
}
