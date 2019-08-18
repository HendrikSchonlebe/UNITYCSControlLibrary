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
        const int ABN_Length = 11;
        const int ABN_TOO_SHORT = 1;
        const int ABN_TOO_LONG = 2;
        const int ABN_NOT_NUMERIC = 3;
        const int ABN_INVALID = 4;

        public String ErrorMessage { get; set; } = string.Empty;

        public Boolean IsBSB(String strInput)
        {
            Boolean logValid = true;

            ErrorMessage = string.Empty;

            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"[0-9][0-9][0-9]\-[0-9][0-9][0-9]");

            try
            {
                logValid = rgx.Match(strInput).Success;
            }
            catch (Exception ex)
            {
                ErrorMessage = "** Operator **\r\n\r\nData Validation - IsBSB " + ex.Message + " !";
            }

            return logValid;
        }
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
        public Boolean Create_GST_Code_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblGSTCodes (";
                strSQL = strSQL + "gst_code char(1) NOT NULL, ";
                strSQL = strSQL + "gst_description nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "gst_rate Float NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate GST Code Table:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
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

        public Boolean Create_Charges_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblCharges (";
                strSQL = strSQL + "charge_description nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "charge_gstcode char(1) NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Charges Table:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
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

        public Boolean Create_Phrases_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblPhrases (phrase_description nvarchar(50) NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Phrases Table:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
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

        public Boolean Create_Sales_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblSales (";
                strSQL = strSQL + "sales_id BigInt NOT NULL IDENTITY, ";
                strSQL = strSQL + "sales_description nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "sales_date datetime NOT NULL, ";
                strSQL = strSQL + "sales_active Bit NOT NULL, ";
                strSQL = strSQL + "sales_datasource nvarchar(50) NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Sales Table:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
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

        public Boolean Create_Agent_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblAgent (";
                strSQL = strSQL + "agent_name1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_name2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_street nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_city nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_state nvarchar(3) NOT NULL, ";
                strSQL = strSQL + "agent_postcode nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "agent_mailingstreet nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_mailingcity nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_mailingstate nvarchar(3) NOT NULL, ";
                strSQL = strSQL + "agent_mailingpostcode nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "agent_telephone nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_fax nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_email nvarchar(128) NOT NULL, ";
                strSQL = strSQL + "agent_abn nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_bankname nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_branch nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_bsb nvarchar(7) NOT NULL, ";
                strSQL = strSQL + "agent_accountno nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_reportpath nvarchar(128) NOT NULL, ";
                strSQL = strSQL + "agent_invoice nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_accountsale nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_unityaccess bit NOT NULL, ";
                strSQL = strSQL + "agent_unitydbname nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_unitysaname nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_unitysapwd nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_commission1 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission2 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission3 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission4 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission5 Float NOT NULL, ";
                strSQL = strSQL + "agent_chargepremium bit NOT NULL, ";
                strSQL = strSQL + "agent_premium1 Float NOT NULL, ";
                strSQL = strSQL + "agent_premium2 Float NOT NULL, ";
                strSQL = strSQL + "agent_premium3 Float NOT NULL, ";
                strSQL = strSQL + "agent_premium4 Float NOT NULL, ";
                strSQL = strSQL + "agent_premium5 Float NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Agent's Table:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
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
                if (AgentTelephone != thisAgent.Rows[0]["agent_telephone"].ToString())
                {
                    strSQL = strSQL + "agent_telephone = '" + MyFormatting.Hyphon(AgentTelephone) + "', ";
                    hasChanged = true;
                }
                if (AgentFax != thisAgent.Rows[0]["agent_fax"].ToString())
                {
                    strSQL = strSQL + "agent_fax = '" + MyFormatting.Hyphon(AgentFax) + "', ";
                    hasChanged = true;
                }
                if (AgentEmail != thisAgent.Rows[0]["agent_email"].ToString())
                {
                    strSQL = strSQL + "agent_email = '" + MyFormatting.Hyphon(AgentEmail) + "', ";
                    hasChanged = true;
                }
                if (AgentABN != thisAgent.Rows[0]["agent_abn"].ToString())
                {
                    strSQL = strSQL + "agent_abn = '" + MyFormatting.Hyphon(AgentABN) + "', ";
                    hasChanged = true;
                }
                if (AgentGSTStatus != Convert.ToBoolean(thisAgent.Rows[0]["agent_gstreg"]))
                {
                    strSQL = strSQL + "agent_gstreg = '" + AgentGSTStatus.ToString() + "', ";
                    hasChanged = true;
                }
                if (AgentACN != thisAgent.Rows[0]["agent_acn"].ToString())
                {
                    strSQL = strSQL + "agent_acn = '" + MyFormatting.Hyphon(AgentACN) + "', ";
                    hasChanged = true;
                }
                if (AgentBankName != thisAgent.Rows[0]["agent_bankname"].ToString())
                {
                    strSQL = strSQL + "agent_bankname = '" + MyFormatting.Hyphon(AgentBankName) + "', ";
                    hasChanged = true;
                }
                if (AgentBranchName != thisAgent.Rows[0]["agent_branch"].ToString())
                {
                    strSQL = strSQL + "agent_branch = '" + MyFormatting.Hyphon(AgentBranchName) + "', ";
                    hasChanged = true;
                }
                if (AgentBSB != thisAgent.Rows[0]["agent_bsb"].ToString())
                {
                    strSQL = strSQL + "agent_bsb = '" + MyFormatting.Hyphon(AgentBSB) + "', ";
                    hasChanged = true;
                }
                if (AgentAccountNumber != thisAgent.Rows[0]["agent_acccountno"].ToString())
                {
                    strSQL = strSQL + "agent_accountno = '" + MyFormatting.Hyphon(AgentAccountNumber) + "', ";
                    hasChanged = true;
                }
                if (AgentReportsPath != thisAgent.Rows[0]["agent_reportpath"].ToString())
                {
                    strSQL = strSQL + "agent_reportpath = '" + MyFormatting.Hyphon(AgentReportsPath) + "', ";
                    hasChanged = true;
                }
                if (AgentInvoiceDocument != thisAgent.Rows[0]["agent_invoice"].ToString())
                {
                    strSQL = strSQL + "agent_invoice = '" + MyFormatting.Hyphon(AgentInvoiceDocument) + "', ";
                    hasChanged = true;
                }
                if (AgentAccountSaleDocument != thisAgent.Rows[0]["agent_accountsale"].ToString())
                {
                    strSQL = strSQL + "agent_accountsale = '" + MyFormatting.Hyphon(AgentAccountSaleDocument) + "', ";
                    hasChanged = true;
                }
                if (AgentUnityAccess != Convert.ToBoolean(thisAgent.Rows[0]["agent_unityaccess"]))
                {
                    strSQL = strSQL + "agent_unityaccess = '" + AgentUnityAccess.ToString() + "', ";
                    hasChanged = true;
                }
                if (AgentUnityDBName != thisAgent.Rows[0]["agent_unitydbname"].ToString())
                {
                    strSQL = strSQL + "agent_unitydbname = '" + MyFormatting.Hyphon(AgentUnityDBName) + "', ";
                    hasChanged = true;
                }
                if (AgentUnitysaName != thisAgent.Rows[0]["agent_unitysaname"].ToString())
                {
                    strSQL = strSQL + "agent_unitysaname = '" + MyFormatting.Hyphon(AgentUnitysaName) + "', ";
                    hasChanged = true;
                }
                if (AgentUnitysapwd != thisAgent.Rows[0]["agent_unitysapwd"].ToString())
                {
                    strSQL = strSQL + "agent_unitysapwd = '" + MyFormatting.Hyphon(AgentUnitysapwd) + "', ";
                    hasChanged = true;
                }
                if (AgentComissionRate1 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission1"]))
                {
                    strSQL = strSQL + "agent_comission1 = " + AgentComissionRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate2 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission2"]))
                {
                    strSQL = strSQL + "agent_comission2 = " + AgentComissionRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate3 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission3"]))
                {
                    strSQL = strSQL + "agent_comission3 = " + AgentComissionRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate4 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission4"]))
                {
                    strSQL = strSQL + "agent_comission4 = " + AgentComissionRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate5 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission5"]))
                {
                    strSQL = strSQL + "agent_comission5 = " + AgentComissionRate5.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentChargePremium != Convert.ToBoolean(thisAgent.Rows[0]["agent_chargepremium"]))
                {
                    strSQL = strSQL + "agent_chargepremium = '" + AgentChargePremium.ToString() + "', ";
                    hasChanged = true;
                }
                if (AgentPremiumRate1 != Convert.ToDouble(thisAgent.Rows[0]["agent_premium1"]))
                {
                    strSQL = strSQL + "agent_premium1 = " + AgentPremiumRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentPremiumRate2 != Convert.ToDouble(thisAgent.Rows[0]["agent_premium2"]))
                {
                    strSQL = strSQL + "agent_premium2 = " + AgentPremiumRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentPremiumRate3 != Convert.ToDouble(thisAgent.Rows[0]["agent_premium3"]))
                {
                    strSQL = strSQL + "agent_premium3 = " + AgentPremiumRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentPremiumRate4 != Convert.ToDouble(thisAgent.Rows[0]["agent_premium4"]))
                {
                    strSQL = strSQL + "agent_premium4 = " + AgentPremiumRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentPremiumRate5 != Convert.ToDouble(thisAgent.Rows[0]["agent_premium5"]))
                {
                    strSQL = strSQL + "agent_premium5 = " + AgentPremiumRate5.ToString() + ", ";
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
        
        #region Control Table
        public Boolean IntegrateUnity { get; set; }
        public Double CommissionRate1 { get; set; }
        public Double CommissionRate2 { get; set; }
        public Double CommissionRate3 { get; set; }
        public Double CommissionRate4 { get; set; }
        public Double CommissionRate5 { get; set; }
        public Boolean ChargePremium { get; set; }
        public Double PremiumRate1 { get; set; }
        public Double PremiumRate2 { get; set; }
        public Double PremiumRate3 { get; set; }
        public Double PremiumRate4 { get; set; }
        public Double PremiumRate5 { get; set; }
        public Int32 NextInvoiceNumber { get; set; }
        public Int32 NextAccountSaleNumber { get; set; }
        public DataTable ThisControlRecord { get; set; } = new DataTable();
        public Boolean Create_Control_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblControl (";
                strSQL = strSQL + "cs_ctrl_unity bit NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_commission1 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_commission2 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_commission3 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_commission4 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_commission5 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_chargepremium bit NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_premium1 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_premium2 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_premium3 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_premium4 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_premium5 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_next_inv BigInt NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_next_acs BigInt NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();

                strSQL = "INSERT INTO tblControl (";
                strSQL = strSQL + "cs_ctrl_unity, ";
                strSQL = strSQL + "cs_ctrl_commission1, ";
                strSQL = strSQL + "cs_ctrl_commission2, ";
                strSQL = strSQL + "cs_ctrl_commission3, ";
                strSQL = strSQL + "cs_ctrl_commission4, ";
                strSQL = strSQL + "cs_ctrl_commission5, ";
                strSQL = strSQL + "cs_ctrl_chargepremium, ";
                strSQL = strSQL + "cs_ctrl_premium1, ";
                strSQL = strSQL + "cs_ctrl_premium2, ";
                strSQL = strSQL + "cs_ctrl_premium3, ";
                strSQL = strSQL + "cs_ctrl_premium4, ";
                strSQL = strSQL + "cs_ctrl_premium5, ";
                strSQL = strSQL + "cs_ctrl_next_inv, ";
                strSQL = strSQL + "cs_ctrl_next_acs) VALUES (";
                strSQL = strSQL + "'False', ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "'False', ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "0.00, ";
                strSQL = strSQL + "1, ";
                strSQL = strSQL + "1)";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nCreate Control Table - More than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Control Table - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Control_Record()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                ThisControlRecord.Clear();

                String strSQL = "SELECT * FROM tblControl";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisControlRecord.Load(rdrGet);
                    isSuccessful = Gather_Control_Record();
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
                catch (Exception ex)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Control Record - " + ex.Message + " !";
                }

                return isSuccessful;
            }
            public Boolean Get_Control_Record(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                ThisControlRecord.Clear();

                String strSQL = "SELECT * FROM tblControl";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisControlRecord.Load(rdrGet);
                    isSuccessful = Gather_Control_Record();
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Control Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Gather_Control_Record()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                IntegrateUnity = Convert.ToBoolean(ThisControlRecord.Rows[0]["cs_ctrl_unity"]);
                CommissionRate1 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Commission1"]);
                CommissionRate2 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Commission2"]);
                CommissionRate3 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Commission3"]);
                CommissionRate4 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Commission4"]);
                CommissionRate5 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Commission5"]);
                ChargePremium = Convert.ToBoolean(ThisControlRecord.Rows[0]["cs_ctrl_chargepremium"]);
                PremiumRate1 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium1"]);
                PremiumRate2 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium2"]);
                PremiumRate3 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium3"]);
                PremiumRate4 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium4"]);
                PremiumRate5 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium5"]);
                NextInvoiceNumber = Convert.ToInt32(ThisControlRecord.Rows[0]["cs_ctrl_next_inv"]);
                NextAccountSaleNumber = Convert.ToInt32(ThisControlRecord.Rows[0]["cs_ctrl_next_acs"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Control Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Control_Record(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblControl SET ";

                if (IntegrateUnity != Convert.ToBoolean(ThisControlRecord.Rows[0]["cs_ctrl_unity"]))
                {
                    strSQL = strSQL + "cs_ctrl_unity = '" + IntegrateUnity.ToString() + "', ";
                    hasChanged = true;
                }
                if (CommissionRate1 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_commission1"]))
                {
                    strSQL = strSQL + "cs_ctrl_commission1 = " + CommissionRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (CommissionRate2 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_commission2"]))
                {
                    strSQL = strSQL + "cs_ctrl_commission2 = " + CommissionRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (CommissionRate3 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_commission3"]))
                {
                    strSQL = strSQL + "cs_ctrl_commission3 = " + CommissionRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (CommissionRate4 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_commission4"]))
                {
                    strSQL = strSQL + "cs_ctrl_commission4 = " + CommissionRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (CommissionRate5 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_commission5"]))
                {
                    strSQL = strSQL + "cs_ctrl_commission5 = " + CommissionRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (ChargePremium != Convert.ToBoolean(ThisControlRecord.Rows[0]["cs_ctrl_chargepremium"]))
                {
                    strSQL = strSQL + "cs_ctrl_chargepremium = '" + ChargePremium.ToString() + "', ";
                    hasChanged = true;
                }
                if (PremiumRate1 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium1"]))
                {
                    strSQL = strSQL + "cs_ctrl_Premium1 = " + PremiumRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (PremiumRate2 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium2"]))
                {
                    strSQL = strSQL + "cs_ctrl_Premium2 = " + PremiumRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (PremiumRate3 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium3"]))
                {
                    strSQL = strSQL + "cs_ctrl_Premium3 = " + PremiumRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (PremiumRate4 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium4"]))
                {
                    strSQL = strSQL + "cs_ctrl_Premium4 = " + PremiumRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (PremiumRate5 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_Premium5"]))
                {
                    strSQL = strSQL + "cs_ctrl_Premium5 = " + PremiumRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (NextInvoiceNumber != Convert.ToInt32(ThisControlRecord.Rows[0]["cs_ctrl_nextinv"]))
                {
                    strSQL = strSQL + "cs_ctrl_nextinv = " + NextInvoiceNumber.ToString() + ", ";
                    hasChanged = true;
                }
                if (NextAccountSaleNumber != Convert.ToInt32(ThisControlRecord.Rows[0]["cs_ctrl_nextacs"]))
                {
                    strSQL = strSQL + "cs_ctrl_nextacs = " + NextAccountSaleNumber.ToString() + ", ";
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
                ErrorMessage = "** Operator **\r\n\r\nUpdate Control Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Catalogue Table
        public Int32 CatalogueVendorId { get; set; }
        public Int32 CatalogueId { get; set; }
        public String CatalogueCode { get; set; }
        public String CatalogueDescription { get; set; }
        public Double CatalogueReserve { get; set; }
        public Boolean CatalogueSold { get; set; }
        public DataTable ThisCatalogueRecord { get; set; } = new DataTable();
        public DataTable CatalogueRecords { get; set; } = new DataTable();

        public Boolean Insert_Catalogue_Record(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblCatalogue (";
                strSQL = strSQL + "cs_cat_vendorid, ";
                strSQL = strSQL + "cs_cat_code, ";
                strSQL = strSQL + "cs_cat_description, ";
                strSQL = strSQL + "cs_cat_reserve, ";
                strSQL = strSQL + "cs_cat_sold) VALUES (";
                strSQL = strSQL + CatalogueVendorId.ToString() + ", ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(CatalogueCode) + "', "; 
                strSQL = strSQL + "'" + MyFormatting.Hyphon(CatalogueDescription) + "', "; 
                strSQL = strSQL + CatalogueReserve.ToString() + ", ";
                strSQL = strSQL + "'" + CatalogueSold.ToString() + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert Catalogue Record - More than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert Catalogue Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Catalogue_Record(Int32 catId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisCatalogueRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblCatalogue WHERE cs_cat_id = " + catId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisCatalogueRecord.Load(rdrGet);
                    isSuccessful = Gather_Catalogue_Record();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record - Catalogue Record with Id " + catId.ToString() + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Catalogue_Record(Int32 catId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisCatalogueRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblCatalogue WHERE cs_cat_id = " + catId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisCatalogueRecord.Load(rdrGet);
                    isSuccessful = Gather_Catalogue_Record();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record - Catalogue Record with Id " + catId.ToString() + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Catalogue_Record(String catCode)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisCatalogueRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblCatalogue WHERE cs_cat_code = '" + catCode + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisCatalogueRecord.Load(rdrGet);
                    isSuccessful = Gather_Catalogue_Record();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record - Catalogue Record with Code " + catCode + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Gather_Catalogue_Record()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                CatalogueId = Convert.ToInt32(ThisCatalogueRecord.Rows[0]["cs_cat_id"]);
                CatalogueVendorId = Convert.ToInt32(ThisCatalogueRecord.Rows[0]["cs_cat_vendorid"]);
                CatalogueCode = ThisCatalogueRecord.Rows[0]["cs_cat_code"].ToString();
                CatalogueDescription = ThisCatalogueRecord.Rows[0]["cs_cat_description"].ToString();
                CatalogueSold = Convert.ToBoolean(ThisCatalogueRecord.Rows[0]["cs_cat_sold"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Catalogue Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Catalogue_Record(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblCatalogue SET ";

                if (CatalogueVendorId != Convert.ToInt32(ThisCatalogueRecord.Rows[0]["cs_cat_vendorid"]))
                {
                    strSQL = strSQL + "cs_cat_vendorid = " + CatalogueVendorId.ToString() + ", ";
                    hasChanged = true;
                }
                if (CatalogueCode != ThisCatalogueRecord.Rows[0]["cs_cat_code"].ToString())
                {
                    strSQL = strSQL + "cs_cat_code = '" + MyFormatting.Hyphon(CatalogueCode) + "', ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2);
                    strSQL = strSQL + " WHERE cs_cat_id = " + CatalogueId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Catalogue Record - More than one Catalogue record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Catalogue Record - " + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Vendor Table
        public Int32 VendorId { get; set; }
        public String VendorUnityCode { get; set; }
        public String VendorName1 { get; set; }
        public String VendorName2 { get; set; }
        public String VendorAddress1 { get; set; }
        public String VendorAddress2 { get; set; }
        public String VendorCity { get; set; }
        public String VendorState { get; set; }
        public String VendorPostCode { get; set; }
        public String VendorTelephone { get; set; }
        public String VendorEmail { get; set; }
        public String VendorABN { get; set; }
        public Boolean VendorGSTStatus { get; set; }
        public Double VendorDefaultCommissionRate { get; set; }
        public DataTable ThisVendorRecord { get; set; } = new DataTable();
        public DataTable VendorRecords { get; set; } = new DataTable();

        #endregion
        #region Buyer Table
        public Int32 BuyerId { get; set; }
        public String BuyerUnityCode { get; set; }
        public String BuyerBidderNumber { get; set; }
        public String BuyerName1 { get; set; }
        public String BuyerName2 { get; set; }
        public String BuyerAddress1 { get; set; }
        public String BuyerAddress2 { get; set; }
        public String BuyerCity { get; set; }
        public String BuyerState { get; set; }
        public String BuyerPostCode { get; set; }
        public String BuyerTelephone { get; set; }
        public String BuyerEmail { get; set; }
        public String BuyerABN { get; set; }

        public DataTable ThisBuyerRecord { get; set; } = new DataTable();
        public DataTable BuyerRecords { get; set; } = new DataTable();

        #endregion
        #region Lot Table
        public Int32 LotId { get; set; }
        public String LotNumber { get; set; }
        public Int32 LotVendorId { get; set; }
        public Int32 LotCatalogueId { get; set; }
        public String LotDescription { get; set; }
        public Double LotQuantity { get; set; }
        public Double LotPrice { get; set; }
        public String LotGSTCode { get; set; }
        public Double LotCommissionRate { get; set; }
        public Double LotCommission { get; set; }
        public Int32 LotBuyerid { get; set; }
        public Double LotPremiumRate { get; set; }
        public Int32 LotInvoiceId { get; set; }
        public Int32 LotAccountSaleId { get; set; }
        public DataTable ThisLotRecord { get; set; } = new DataTable();
        public DataTable LotRecords { get; set; } = new DataTable();

        #endregion
    }
}
