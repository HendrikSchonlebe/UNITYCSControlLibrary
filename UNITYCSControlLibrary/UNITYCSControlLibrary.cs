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
                ErrorMessage = "** Operator **\r\n\r\nData Validation: IsBSB " + ex.Message + " !";
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
                ErrorMessage = "** Operator **\r\n\r\nData Validation: IsEmailAddress " + ex.Message + " !";
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
                ErrorMessage = ERROR_MESSAGES[_errorCode] + " !";

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
                    strSQL = strSQL.Substring(0, strSQL.Length - 2) + " WHERE gst_code = '" + GSTCode + "'";
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
        public Boolean Delete_GST_Code(String gstCode, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblGSTCodes WHERE gst_code = '" + gstCode + "'";
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
        public DataTable ChargesListWithGST { get; set; } = new DataTable();

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
                strSQL = strSQL + "charge_id Bigint NOT NULL IDENTITY, ";
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
                if (cmdInsert.ExecuteNonQuery() == 1)
                {
                    isSuccessful = Get_Charge(ChargeDescription, trnEnvelope);
                }
                else
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
                rdrGet.Close();
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
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Charge(String chargeDescription, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblCharges WHERE charge_description = '" + MyFormatting.Hyphon(chargeDescription) + "'";
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Charge:\r\n\r\nCharge with Description " + chargeDescription + " not found !";
                }
                rdrGet.Close();
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
        public Boolean Get_Charges_List()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ChargesList.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblCharges ORDER BY charge_description";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ChargesList.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Charges List:\r\n\r\n" + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Charges_List_With_GST()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ChargesListWithGST.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblCharges INNER JOIN tblGSTCodes ON tblGSTCodes.gst_code = tblCharges.charge_gstcode ORDER BY tblCharges.charge_description";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ChargesListWithGST.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Charges List with GST:\r\n\r\n" + ex.Message + " !";
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
                String strSQL = "CREATE TABLE tblPhrases (";
                strSQL = strSQL + "phrase_id Bigint NOT NULL IDENTITY, ";
                strSQL = strSQL + "phrase_description nvarchar(500) NOT NULL)";
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
                if (cmdInsert.ExecuteNonQuery() == 1)
                {
                    isSuccessful = Get_Phrase(PhraseDescription, trnEnvelope);
                }
                else
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
        public Boolean Get_Phrase(Int32 phraseId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            thisPhrase.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblPhrases WHERE phrase_id = " + phraseId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
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
        public Boolean Get_Phrase(String phraseText, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            thisPhrase.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblPhrases WHERE phrase_description = '" + MyFormatting.Hyphon(phraseText) + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
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
                strSQL = strSQL + "CONVERT(datetime, '" + SaleDate.ToString() + "', 103), ";
                strSQL = strSQL + "'" + SaleIsActive.ToString() + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(SaleDataSource) + "')";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Sale:\r\n\r\nMore than one record would be inserted !";
                }
                else
                {
                    isSuccessful = Get_Sale(SaleDescription, trnEnvelope);
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
        public Boolean Get_Sale(String saleName, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisSale.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblSales WHERE sales_description = '" + MyFormatting.Hyphon(saleName) + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisSale.Load(rdrGet);
                    isSuccessful = Gather_Sale();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Sale:\r\n\r\nSale " + saleName + " not found !";
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
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nList of Sales:\r\n\r\n" + ex.Message + " !";
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
        public String AgentBankName { get; set; }
        public String AgentBranchName { get; set; }
        public String AgentBSB { get; set; }
        public String AgentAccountNumber { get; set; }
        public Double AgentComissionRate1 { get; set; }
        public Double AgentComissionRate2 { get; set; }
        public Double AgentComissionRate3 { get; set; }
        public Double AgentComissionRate4 { get; set; }
        public Double AgentComissionRate5 { get; set; }
        public Double AgentRebateRate1 { get; set; }
        public Double AgentRebateRate2 { get; set; }
        public Double AgentRebateRate3 { get; set; }
        public Double AgentRebateRate4 { get; set; }
        public Double AgentRebateRate5 { get; set; }
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
        public String AgentUnityServerName { get; set; }
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
                strSQL = strSQL + "agent_gstreg bit NOT NULL, ";
                strSQL = strSQL + "agent_accountno nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_reportpath nvarchar(128) NOT NULL, ";
                strSQL = strSQL + "agent_invoice nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_accountsale nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "agent_unityaccess bit NOT NULL, ";
                strSQL = strSQL + "agent_unityserver nvarchar(30) NOT NULL, ";
                strSQL = strSQL + "agent_unitydbname nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_unitysaname nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_unitysapwd nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "agent_commission1 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission2 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission3 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission4 Float NOT NULL, ";
                strSQL = strSQL + "agent_commission5 Float NOT NULL, ";
                strSQL = strSQL + "agent_rebate1 Float NOT NULL, ";
                strSQL = strSQL + "agent_rebate2 Float NOT NULL, ";
                strSQL = strSQL + "agent_rebate3 Float NOT NULL, ";
                strSQL = strSQL + "agent_rebate4 Float NOT NULL, ";
                strSQL = strSQL + "agent_rebate5 Float NOT NULL, ";
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
        public Boolean Insert_Agent(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblAgent (";
                strSQL = strSQL + "agent_name1, ";
                strSQL = strSQL + "agent_name2, ";
                strSQL = strSQL + "agent_street, ";
                strSQL = strSQL + "agent_city, ";
                strSQL = strSQL + "agent_state, ";
                strSQL = strSQL + "agent_postcode, ";
                strSQL = strSQL + "agent_mailingstreet, ";
                strSQL = strSQL + "agent_mailingcity, ";
                strSQL = strSQL + "agent_mailingstate, ";
                strSQL = strSQL + "agent_mailingpostcode, ";
                strSQL = strSQL + "agent_telephone, ";
                strSQL = strSQL + "agent_fax, ";
                strSQL = strSQL + "agent_email, ";
                strSQL = strSQL + "agent_abn, ";
                strSQL = strSQL + "agent_gstreg, ";
                strSQL = strSQL + "agent_bankname, ";
                strSQL = strSQL + "agent_branch, ";
                strSQL = strSQL + "agent_bsb, ";
                strSQL = strSQL + "agent_accountno, ";
                strSQL = strSQL + "agent_reportpath, ";
                strSQL = strSQL + "agent_invoice, ";
                strSQL = strSQL + "agent_accountsale, ";
                strSQL = strSQL + "agent_unityaccess, ";
                strSQL = strSQL + "agent_unityserver, ";
                strSQL = strSQL + "agent_unitydbname, ";
                strSQL = strSQL + "agent_unitysaname, ";
                strSQL = strSQL + "agent_unitysapwd, ";
                strSQL = strSQL + "agent_commission1, ";
                strSQL = strSQL + "agent_commission2, ";
                strSQL = strSQL + "agent_commission3, ";
                strSQL = strSQL + "agent_commission4, ";
                strSQL = strSQL + "agent_commission5, ";
                strSQL = strSQL + "agent_rebate1, ";
                strSQL = strSQL + "agent_rebate2, ";
                strSQL = strSQL + "agent_rebate3, ";
                strSQL = strSQL + "agent_rebate4, ";
                strSQL = strSQL + "agent_rebate5, ";
                strSQL = strSQL + "agent_chargepremium, ";
                strSQL = strSQL + "agent_premium1, ";
                strSQL = strSQL + "agent_premium2, ";
                strSQL = strSQL + "agent_premium3, ";
                strSQL = strSQL + "agent_premium4, ";
                strSQL = strSQL + "agent_premium5) VALUES (";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentName1) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentName2) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentStreetAddress) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentCityAddress) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentState) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentPostCode) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentMailingStreet) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentMailingCity) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentMailingState) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentMailingPostCode) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentTelephone) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentFax) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentEmail) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentABN) + "', ";
                strSQL = strSQL + "'" + AgentGSTStatus.ToString() + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentBankName) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentBranchName) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentBSB) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentAccountNumber) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentReportsPath) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentInvoiceDocument) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentAccountSaleDocument) + "', ";
                strSQL = strSQL + "'" + AgentUnityAccess.ToString() + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentUnityServerName) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentUnityDBName) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentUnitysaName) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(AgentUnitysapwd) + "', ";
                strSQL = strSQL + AgentComissionRate1.ToString() + ", ";
                strSQL = strSQL + AgentComissionRate2.ToString() + ", ";
                strSQL = strSQL + AgentComissionRate3.ToString() + ", ";
                strSQL = strSQL + AgentComissionRate4.ToString() + ", ";
                strSQL = strSQL + AgentComissionRate5.ToString() + ", ";
                strSQL = strSQL + AgentRebateRate1.ToString() + ", ";
                strSQL = strSQL + AgentRebateRate2.ToString() + ", ";
                strSQL = strSQL + AgentRebateRate3.ToString() + ", ";
                strSQL = strSQL + AgentRebateRate4.ToString() + ", ";
                strSQL = strSQL + AgentRebateRate5.ToString() + ", ";
                strSQL = strSQL + "'" + AgentChargePremium.ToString() + "', ";
                strSQL = strSQL + AgentPremiumRate1.ToString() + ", ";
                strSQL = strSQL + AgentPremiumRate2.ToString() + ", ";
                strSQL = strSQL + AgentPremiumRate3.ToString() + ", ";
                strSQL = strSQL + AgentPremiumRate4.ToString() + ", ";
                strSQL = strSQL + AgentPremiumRate5.ToString() + ")";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert Agent:\r\n\r\nMore than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert Agent:\r\n\r\n" + ex.Message + " !";
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
                AgentBankName = thisAgent.Rows[0]["agent_bankname"].ToString();
                AgentBranchName = thisAgent.Rows[0]["agent_branch"].ToString();
                AgentBSB = thisAgent.Rows[0]["agent_bsb"].ToString();
                AgentAccountNumber = thisAgent.Rows[0]["agent_accountno"].ToString();
                AgentReportsPath = thisAgent.Rows[0]["agent_reportpath"].ToString();
                AgentInvoiceDocument = thisAgent.Rows[0]["agent_invoice"].ToString();
                AgentAccountSaleDocument = thisAgent.Rows[0]["agent_accountsale"].ToString();
                AgentUnityAccess = Convert.ToBoolean(thisAgent.Rows[0]["agent_unityaccess"]);
                AgentUnityServerName = thisAgent.Rows[0]["agent_unityserver"].ToString();
                AgentUnityDBName = thisAgent.Rows[0]["agent_unitydbname"].ToString();
                AgentUnitysaName = thisAgent.Rows[0]["agent_unitysaname"].ToString();
                AgentUnitysapwd = thisAgent.Rows[0]["agent_unitysapwd"].ToString();
                AgentComissionRate1 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission1"]);
                AgentComissionRate2 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission2"]);
                AgentComissionRate3 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission3"]);
                AgentComissionRate4 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission4"]);
                AgentComissionRate5 = Convert.ToDouble(thisAgent.Rows[0]["agent_commission5"]);
                AgentRebateRate1 = Convert.ToDouble(thisAgent.Rows[0]["agent_rebate1"]);
                AgentRebateRate2 = Convert.ToDouble(thisAgent.Rows[0]["agent_rebate2"]);
                AgentRebateRate3 = Convert.ToDouble(thisAgent.Rows[0]["agent_rebate3"]);
                AgentRebateRate4 = Convert.ToDouble(thisAgent.Rows[0]["agent_rebate4"]);
                AgentRebateRate5 = Convert.ToDouble(thisAgent.Rows[0]["agent_rebate5"]);
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
                if (AgentAccountNumber != thisAgent.Rows[0]["agent_accountno"].ToString())
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
                if (AgentUnityServerName != thisAgent.Rows[0]["agent_unityserver"].ToString())
                {
                    strSQL = strSQL + "agent_unityserver = '" + MyFormatting.Hyphon(AgentUnityServerName) + "', ";
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
                    strSQL = strSQL + "agent_commission1 = " + AgentComissionRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate2 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission2"]))
                {
                    strSQL = strSQL + "agent_commission2 = " + AgentComissionRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate3 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission3"]))
                {
                    strSQL = strSQL + "agent_commission3 = " + AgentComissionRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate4 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission4"]))
                {
                    strSQL = strSQL + "agent_commission4 = " + AgentComissionRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentComissionRate5 != Convert.ToDouble(thisAgent.Rows[0]["agent_commission5"]))
                {
                    strSQL = strSQL + "agent_commission5 = " + AgentComissionRate5.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentRebateRate1 != Convert.ToDouble(thisAgent.Rows[0]["agent_rebate1"]))
                {
                    strSQL = strSQL + "agent_rebate1 = " + AgentRebateRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentRebateRate2 != Convert.ToDouble(thisAgent.Rows[0]["agent_rebate2"]))
                {
                    strSQL = strSQL + "agent_rebate2 = " + AgentRebateRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentRebateRate3 != Convert.ToDouble(thisAgent.Rows[0]["agent_rebate3"]))
                {
                    strSQL = strSQL + "agent_rebate3 = " + AgentRebateRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentRebateRate4 != Convert.ToDouble(thisAgent.Rows[0]["agent_rebate4"]))
                {
                    strSQL = strSQL + "agent_rebate4 = " + AgentRebateRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (AgentRebateRate5 != Convert.ToDouble(thisAgent.Rows[0]["agent_rebate5"]))
                {
                    strSQL = strSQL + "agent_rebate5 = " + AgentRebateRate5.ToString() + ", ";
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
        public Double RebateRate1 { get; set; }
        public Double RebateRate2 { get; set; }
        public Double RebateRate3 { get; set; }
        public Double RebateRate4 { get; set; }
        public Double RebateRate5 { get; set; }
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
                strSQL = strSQL + "cs_ctrl_rebate1 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_rebate2 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_rebate3 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_rebate4 Float NOT NULL, ";
                strSQL = strSQL + "cs_ctrl_rebate5 Float NOT NULL, ";
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
                strSQL = strSQL + "cs_ctrl_rebate1, ";
                strSQL = strSQL + "cs_ctrl_rebate2, ";
                strSQL = strSQL + "cs_ctrl_rebate3, ";
                strSQL = strSQL + "cs_ctrl_rebate4, ";
                strSQL = strSQL + "cs_ctrl_rebate5, ";
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
                    ErrorMessage = "** Operator **\r\n\r\nCreate Control Table: More than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Control Table: " + ex.Message + " !";
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Control Record: " + ex.Message + " !";
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
                ErrorMessage = "** Operator **\r\n\r\nGet Control Record: " + ex.Message + " !";
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
                RebateRate1 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate1"]);
                RebateRate2 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate2"]);
                RebateRate3 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate3"]);
                RebateRate4 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate4"]);
                RebateRate5 = Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate5"]);
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
                ErrorMessage = "** Operator **\r\n\r\nGather Control Record: " + ex.Message + " !";
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
                    strSQL = strSQL + "cs_ctrl_commission5 = " + CommissionRate5.ToString() + ", ";
                    hasChanged = true;
                }
                if (RebateRate1 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate1"]))
                {
                    strSQL = strSQL + "cs_ctrl_rebate1 = " + RebateRate1.ToString() + ", ";
                    hasChanged = true;
                }
                if (RebateRate2 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate2"]))
                {
                    strSQL = strSQL + "cs_ctrl_rebate2 = " + RebateRate2.ToString() + ", ";
                    hasChanged = true;
                }
                if (RebateRate3 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate3"]))
                {
                    strSQL = strSQL + "cs_ctrl_rebate3 = " + RebateRate3.ToString() + ", ";
                    hasChanged = true;
                }
                if (RebateRate4 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate4"]))
                {
                    strSQL = strSQL + "cs_ctrl_rebate4 = " + RebateRate4.ToString() + ", ";
                    hasChanged = true;
                }
                if (RebateRate5 != Convert.ToDouble(ThisControlRecord.Rows[0]["cs_ctrl_rebate5"]))
                {
                    strSQL = strSQL + "cs_ctrl_rebate5 = " + RebateRate5.ToString() + ", ";
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
                if (NextInvoiceNumber != Convert.ToInt32(ThisControlRecord.Rows[0]["cs_ctrl_next_inv"]))
                {
                    strSQL = strSQL + "cs_ctrl_next_inv = " + NextInvoiceNumber.ToString() + ", ";
                    hasChanged = true;
                }
                if (NextAccountSaleNumber != Convert.ToInt32(ThisControlRecord.Rows[0]["cs_ctrl_next_acs"]))
                {
                    strSQL = strSQL + "cs_ctrl_next_acs = " + NextAccountSaleNumber.ToString() + ", ";
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
                ErrorMessage = "** Operator **\r\n\r\nUpdate Control Record: " + ex.Message + " !";
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

        public Boolean Create_Catalogue_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblCatalogue (";
                strSQL = strSQL + "cs_cat_id Bigint NOT NULL IDENTITY, ";
                strSQL = strSQL + "cs_cat_vendorid Bigint NOT NULL, ";
                strSQL = strSQL + "cs_cat_code nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_cat_description nvarchar(200) NOT NULL, ";
                strSQL = strSQL + "cs_cat_reserve float NOT NULL, ";
                strSQL = strSQL + "cs_cat_sold bit NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Catalogue Table: " + ex.Message + " !";
            }

            return isSuccessful;
        }
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
                    ErrorMessage = "** Operator **\r\n\r\nInsert Catalogue Record: More than one record would be inserted !";
                }
                else
                {
                    isSuccessful = Get_Catalogue_Record(CatalogueCode, trnEnvelope);
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert Catalogue Record: " + ex.Message + " !";
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: Catalogue Record with Id " + catId.ToString() + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: " + ex.Message + " !";
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: Catalogue Record with Id " + catId.ToString() + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: " + ex.Message + " !";
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: Catalogue Record with Code " + catCode + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Catalogue_Record(String catCode, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisCatalogueRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblCatalogue WHERE cs_cat_code = '" + catCode + "'";
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
                    ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: Catalogue Record with Code " + catCode + " not found !";
                }
                rdrGet.Close();
                cmdGet.Dispose();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Record: " + ex.Message + " !";
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
                CatalogueReserve = Convert.ToDouble(ThisCatalogueRecord.Rows[0]["cs_cat_reserve"]);
                CatalogueSold = Convert.ToBoolean(ThisCatalogueRecord.Rows[0]["cs_cat_sold"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Catalogue Record: " + ex.Message + " !";
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
                if (CatalogueDescription !=  ThisCatalogueRecord.Rows[0]["cs_cat_description"].ToString())
                {
                    strSQL = strSQL + "cs_cat_description = '" + MyFormatting.Hyphon(CatalogueDescription) + "', ";
                    hasChanged = true;
                }
                if (CatalogueReserve != Convert.ToDouble(ThisCatalogueRecord.Rows[0]["cs_cat_reserve"]))
                {
                    strSQL = strSQL + "cs_cat_reserve = " + CatalogueReserve.ToString() + ", ";
                    hasChanged = true;
                }
                if (CatalogueSold != Convert.ToBoolean(ThisCatalogueRecord.Rows[0]["cs_cat_sold"]))
                {
                    strSQL = strSQL + "cs_cat_sold = '" + CatalogueSold.ToString() + "', ";
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
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Catalogue Record: More than one Catalogue record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Catalogue Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Catalogue_Record(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblCatalogue WHERE cs_cat_id = " + CatalogueId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Catalogue Record: More than one Catalogue record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Catalogue Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Catalogue_Records(Int32 vendorId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            CatalogueRecords.Clear();

            try
            {
                String strSQL = "SELECT tblCatalogue.*, tblVendors.* FROM tblCatalogue ";
                strSQL = strSQL + "INNER JOIN tblVendors ON tblCatalogue.cs_cat_vendorid = tblVendors.cs_vendor_id ";
                if (vendorId > 0)
                    strSQL = strSQL + "WHERE tblCatalogue.cs_cat_vendorid = " + vendorId.ToString() + " ";
                strSQL = strSQL + "ORDER BY tblCatalogue.cs_cat_code";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    CatalogueRecords.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Catalogue Records: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Vendor Table
        public Int32 VendorId { get; set; }
        public String VendorGUID { get; set; }
        public String VendorUnityCode { get; set; }
        public String VendorName1 { get; set; }
        public String VendorName2 { get; set; }
        public String VendorAddress1 { get; set; }
        public String VendorAddress2 { get; set; }
        public String VendorCity { get; set; }
        public String VendorState { get; set; }
        public String VendorPostCode { get; set; }
        public String VendorTelephone { get; set; }
        public String VendorMobile { get; set; }
        public String VendorEmail { get; set; }
        public String VendorABN { get; set; }
        public Boolean VendorGSTStatus { get; set; }
        public Double VendorDefaultCommissionRate { get; set; }
        public DataTable ThisVendorRecord { get; set; } = new DataTable();
        public DataTable VendorRecords { get; set; } = new DataTable();

        public Boolean Create_Vendor_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblVendors (";
                strSQL = strSQL + "cs_vendor_id Bigint NOT NULL IDENTITY, ";
                strSQL = strSQL + "cs_vendor_guid nvarchar(36) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_unity nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_name1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_name2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_address1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_address2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_city nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_state nvarchar(3) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_postcode nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_telephone nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_mobile nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_email nvarchar(128) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_abn nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_vendor_gststatus bit NOT NULL, ";
                strSQL = strSQL + "cs_vendor_commissionrate float NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Vendors Table: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Insert_Vendor(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblVendors (";
                strSQL = strSQL + "cs_vendor_guid, ";
                strSQL = strSQL + "cs_vendor_unity, ";
                strSQL = strSQL + "cs_vendor_name1, ";
                strSQL = strSQL + "cs_vendor_name2, ";
                strSQL = strSQL + "cs_vendor_address1, ";
                strSQL = strSQL + "cs_vendor_address2, ";
                strSQL = strSQL + "cs_vendor_city, ";
                strSQL = strSQL + "cs_vendor_state, ";
                strSQL = strSQL + "cs_vendor_postcode, ";
                strSQL = strSQL + "cs_vendor_telephone, ";
                strSQL = strSQL + "cs_vendor_mobile, ";
                strSQL = strSQL + "cs_vendor_email, ";
                strSQL = strSQL + "cs_vendor_abn, ";
                strSQL = strSQL + "cs_vendor_gststatus, ";
                strSQL = strSQL + "cs_vendor_commissionrate) VALUES (";
                strSQL = strSQL + "'" + VendorGUID + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorUnityCode.ToString()) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorName1) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorName2) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorAddress1) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorAddress2) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorCity) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorState) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorPostCode) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorTelephone) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorMobile) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorEmail) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(VendorABN) + "', ";
                strSQL = strSQL + "'" + VendorGSTStatus.ToString() + "', ";
                strSQL = strSQL + VendorDefaultCommissionRate.ToString() + ")";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert Vendor Record: More than one record would be inserted !";
                }
                else
                {
                    isSuccessful = Get_Vendor(VendorGUID, trnEnvelope);
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Vendor(Int32 vendorId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                ThisVendorRecord.Clear();

                String strSQL = "SELECT * FROM tblVendors WHERE cs_vendor_id = " + vendorId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisVendorRecord.Load(rdrGet);
                    isSuccessful = Gather_Vendor();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Vendor Record: Vendor with Id " + vendorId.ToString() + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Vendor(Int32 vendorId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                ThisVendorRecord.Clear();

                String strSQL = "SELECT * FROM tblVendors WHERE cs_vendor_id = " + vendorId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisVendorRecord.Load(rdrGet);
                    isSuccessful = Gather_Vendor();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Vendor Record: Vendor with Id " + vendorId.ToString() + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Vendor(String thisVendor, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                ThisVendorRecord.Clear();

                String strSQL = "SELECT * FROM tblVendors WHERE cs_vendor_guid = '" + thisVendor + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisVendorRecord.Load(rdrGet);
                    isSuccessful = Gather_Vendor();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Vendor Record: Vendor " + thisVendor + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Gather_Vendor()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                VendorId = Convert.ToInt32(ThisVendorRecord.Rows[0]["cs_vendor_id"]);
                VendorGUID = ThisVendorRecord.Rows[0]["cs_vendor_guid"].ToString();
                VendorUnityCode = ThisVendorRecord.Rows[0]["cs_vendor_unity"].ToString();
                VendorName1 = ThisVendorRecord.Rows[0]["cs_vendor_name1"].ToString();
                VendorName2 = ThisVendorRecord.Rows[0]["cs_vendor_name2"].ToString();
                VendorAddress1 = ThisVendorRecord.Rows[0]["cs_vendor_address1"].ToString();
                VendorAddress2 = ThisVendorRecord.Rows[0]["cs_vendor_address2"].ToString();
                VendorCity = ThisVendorRecord.Rows[0]["cs_vendor_city"].ToString();
                VendorState = ThisVendorRecord.Rows[0]["cs_vendor_state"].ToString();
                VendorPostCode = ThisVendorRecord.Rows[0]["cs_vendor_postcode"].ToString();
                VendorTelephone = ThisVendorRecord.Rows[0]["cs_vendor_telephone"].ToString();
                VendorMobile = ThisVendorRecord.Rows[0]["cs_vendor_mobile"].ToString();
                VendorEmail = ThisVendorRecord.Rows[0]["cs_vendor_email"].ToString();
                VendorABN = ThisVendorRecord.Rows[0]["cs_vendor_abn"].ToString();
                VendorGSTStatus = Convert.ToBoolean(ThisVendorRecord.Rows[0]["cs_vendor_gststatus"]);
                VendorDefaultCommissionRate = Convert.ToDouble(ThisVendorRecord.Rows[0]["cs_vendor_commissionrate"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Vendor(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblVendors SET ";
                if (VendorUnityCode != ThisVendorRecord.Rows[0]["cs_vendor_unity"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_unity = '" + MyFormatting.Hyphon(VendorUnityCode) + "', ";
                    hasChanged = true;
                }
                if (VendorName1 != ThisVendorRecord.Rows[0]["cs_vendor_name1"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_name1 = '" + MyFormatting.Hyphon(VendorName1) + "', ";
                    hasChanged = true;
                }
                if (VendorName2 != ThisVendorRecord.Rows[0]["cs_vendor_name2"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_name2 = '" + MyFormatting.Hyphon(VendorName2) + "', ";
                    hasChanged = true;
                }
                if (VendorAddress1 != ThisVendorRecord.Rows[0]["cs_vendor_address1"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_address1 = '" + MyFormatting.Hyphon(VendorAddress1) + "', ";
                    hasChanged = true;
                }
                if (VendorAddress2 != ThisVendorRecord.Rows[0]["cs_vendor_address2"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_address2 = '" + MyFormatting.Hyphon(VendorAddress2) + "', ";
                    hasChanged = true;
                }
                if (VendorCity != ThisVendorRecord.Rows[0]["cs_vendor_city"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_city = '" + MyFormatting.Hyphon(VendorCity) + "', ";
                    hasChanged = true;
                }
                if (VendorState != ThisVendorRecord.Rows[0]["cs_vendor_state"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_state = '" + MyFormatting.Hyphon(VendorState) + "', ";
                    hasChanged = true;
                }
                if (VendorPostCode != ThisVendorRecord.Rows[0]["cs_vendor_postcode"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_postcode = '" + MyFormatting.Hyphon(VendorPostCode) + "', ";
                    hasChanged = true;
                }
                if (VendorTelephone != ThisVendorRecord.Rows[0]["cs_vendor_telephone"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_telephone = '" + MyFormatting.Hyphon(VendorTelephone) + "', ";
                    hasChanged = true;
                }
                if (VendorMobile != ThisVendorRecord.Rows[0]["cs_vendor_mobile"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_mobile = '" + MyFormatting.Hyphon(VendorMobile) + "', ";
                    hasChanged = true;
                }
                if (VendorEmail != ThisVendorRecord.Rows[0]["cs_vendor_email"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_email = '" + MyFormatting.Hyphon(VendorEmail) + "', ";
                    hasChanged = true;
                }
                if (VendorABN != ThisVendorRecord.Rows[0]["cs_vendor_abn"].ToString())
                {
                    strSQL = strSQL + "cs_vendor_abn = '" + MyFormatting.Hyphon(VendorABN) + "', ";
                    hasChanged = true;
                }
                if (VendorGSTStatus != Convert.ToBoolean(ThisVendorRecord.Rows[0]["cs_vendor_gststatus"]))
                {
                    strSQL = strSQL + "cs_vendor_gststatus = '" + VendorGSTStatus.ToString() + "', ";
                    hasChanged = true;
                }
                if (VendorDefaultCommissionRate != Convert.ToDouble(ThisVendorRecord.Rows[0]["cs_vendor_commissionrate"]))
                {
                    strSQL = strSQL + "cs_vendor_commissionrate = " + VendorDefaultCommissionRate.ToString() + ", ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2);
                    strSQL = strSQL + " WHERE cs_vendor_id = " + VendorId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Vendor Record: More than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Can_Delete_Vendor(Int32 vendorId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblCatalogue WHERE cs_cat_vendorid  = " + vendorId.ToString();
                SqlCommand cmdGetC = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGetC = cmdGetC.ExecuteReader();
                if (rdrGetC.HasRows == true)
                {
                    isSuccessful = false;
                    ErrorMessage = ErrorMessage + "There are Catalogue records associated with this Vendor !\r\n";
                }
                rdrGetC.Close();

                strSQL = "SELECT * FROM tblLots WHERE cs_lot_vendorid = " + vendorId.ToString();
                SqlCommand cmdGetL = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGetL = cmdGetL.ExecuteReader();
                if (rdrGetL.HasRows == true)
                {
                    isSuccessful = false;
                    ErrorMessage = ErrorMessage + "There are Auction Lots associated with this Vendor !\r\n";
                }
                rdrGetL.Close();

                if (isSuccessful == false)
                {
                    ErrorMessage = "** Operator **\r\n\r\nTest if Vendor can be Deleted:\r\n\r\n" + ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nTest if Vendor can be Deleted: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Vendor(SqlTransaction trnEnvelope, Int32 vendorId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblVendors WHERE cs_vendor_id = " + vendorId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Vendor Record: More than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Vendor Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Int32 Vendors_In_Sale()
        {
            Int32 vendorCount = 0;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblVendors";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    DataTable myVendors = new DataTable();
                    myVendors.Load(rdrGet);
                    vendorCount = myVendors.Rows.Count;
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = "** Operator **\r\n\r\nGet Vendor Count: " + ex.Message + " !";
            }

            return vendorCount;
        }
        public Boolean Get_List_Of_Vendors()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            VendorRecords.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblVendors ORDER BY cs_vendor_name1";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    VendorRecords.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet List of Vendors: " + ex.Message + " !";
            }

            return isSuccessful;
        }
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
        public String BuyerMobile { get; set; }
        public String BuyerEmail { get; set; }
        public Double BuyerDefaultPremium { get; set; }
        public Double BuyerDefaultRebate { get; set; }
        public DataTable ThisBuyerRecord { get; set; } = new DataTable();
        public DataTable BuyerRecords { get; set; } = new DataTable();

        public Boolean Create_Buyers_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblBuyers (";
                strSQL = strSQL + "cs_buyer_id Bigint NOT NULL IDENTITY, ";
                strSQL = strSQL + "cs_buyer_bidderno nvarchar(10) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_unity nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_name1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_name2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_address1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_address2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_city nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_state nvarchar(3) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_postcode nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_telephone nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_mobile nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_email nvarchar(128) NOT NULL, ";
                strSQL = strSQL + "cs_buyer_rebate float NOT NULL, ";
                strSQL = strSQL + "cs_buyer_premium float NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Buyers Table: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Insert_Buyer(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblBuyers (";
                strSQL = strSQL + "cs_buyer_bidderno, ";
                strSQL = strSQL + "cs_buyer_unity, ";
                strSQL = strSQL + "cs_buyer_name1, ";
                strSQL = strSQL + "cs_buyer_name2, ";
                strSQL = strSQL + "cs_buyer_address1, ";
                strSQL = strSQL + "cs_buyer_address2, ";
                strSQL = strSQL + "cs_buyer_city, ";
                strSQL = strSQL + "cs_buyer_state, ";
                strSQL = strSQL + "cs_buyer_postcode, ";
                strSQL = strSQL + "cs_buyer_telephone, ";
                strSQL = strSQL + "cs_buyer_mobile, ";
                strSQL = strSQL + "cs_buyer_email, ";
                strSQL = strSQL + "cs_buyer_rebate, ";
                strSQL = strSQL + "cs_buyer_premium) VALUES (";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerBidderNumber) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerUnityCode) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerName1) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerName2) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerAddress1) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerAddress2) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerCity) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerState) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerPostCode) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerTelephone) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerMobile) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(BuyerEmail) + "', ";
                strSQL = strSQL + BuyerDefaultRebate.ToString() + ", ";
                strSQL = strSQL + BuyerDefaultPremium.ToString() + ")";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Buyer: More than one reocrd would be inserted !";
                }
                else
                {
                    isSuccessful = Get_Buyer(BuyerBidderNumber, trnEnvelope);
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert New Buyer: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Is_Duplicate_BidderNo(String bidderNo, Int32 buyerId)
        {
            Boolean isDuplicate = false;
            DataTable myBuyer = new DataTable();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers WHERE cs_buyer_bidderno = '" + MyFormatting.Hyphon(bidderNo) + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    myBuyer.Load(rdrGet);
                    if (Convert.ToInt32(myBuyer.Rows[0]["cs_buyer_id"]) != buyerId)
                        isDuplicate = true;
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = "** Operator **\r\n\r\nBidder Number Duplication Test: " + ex.Message + " !";
            }

            return isDuplicate;
        }
        public Boolean Get_Buyer(Int32 buyerId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ThisBuyerRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers WHERE cs_buyer_id = " + buyerId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisBuyerRecord.Load(rdrGet);
                    isSuccessful = Gather_Buyer();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Buyer: Buyer with Id " + buyerId.ToString() + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Buyer: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Buyer(String bidderNo)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ThisBuyerRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers WHERE cs_buyer_bidderno = '" + bidderNo + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisBuyerRecord.Load(rdrGet);
                    isSuccessful = Gather_Buyer();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Buyer: Buyer with Bidder No " + bidderNo + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Buyer: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Buyer(Int32 buyerId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ThisBuyerRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers WHERE cs_buyer_id = " + buyerId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisBuyerRecord.Load(rdrGet);
                    isSuccessful = Gather_Buyer();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Buyer: Buyer with Id " + buyerId.ToString() + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Buyer: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Buyer(String bidderNo, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ThisBuyerRecord.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers WHERE cs_buyer_bidderno = '" + bidderNo + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisBuyerRecord.Load(rdrGet);
                    isSuccessful = Gather_Buyer();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Buyer: Buyer with Bidder No " + bidderNo + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Buyer: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Gather_Buyer()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                BuyerId = Convert.ToInt32(ThisBuyerRecord.Rows[0]["cs_buyer_id"]);
                BuyerBidderNumber = ThisBuyerRecord.Rows[0]["cs_buyer_bidderno"].ToString();
                BuyerUnityCode = ThisBuyerRecord.Rows[0]["cs_buyer_unity"].ToString();
                BuyerName1 = ThisBuyerRecord.Rows[0]["cs_buyer_name1"].ToString();
                BuyerName2 = ThisBuyerRecord.Rows[0]["cs_buyer_name2"].ToString();
                BuyerAddress1 = ThisBuyerRecord.Rows[0]["cs_buyer_address1"].ToString();
                BuyerAddress2 = ThisBuyerRecord.Rows[0]["cs_buyer_address2"].ToString();
                BuyerCity = ThisBuyerRecord.Rows[0]["cs_buyer_city"].ToString();
                BuyerState = ThisBuyerRecord.Rows[0]["cs_buyer_state"].ToString();
                BuyerPostCode = ThisBuyerRecord.Rows[0]["cs_buyer_postcode"].ToString();
                BuyerTelephone = ThisBuyerRecord.Rows[0]["cs_buyer_telephone"].ToString();
                BuyerMobile = ThisBuyerRecord.Rows[0]["cs_buyer_mobile"].ToString();
                BuyerEmail = ThisBuyerRecord.Rows[0]["cs_buyer_email"].ToString();
                BuyerDefaultRebate = Convert.ToDouble(ThisBuyerRecord.Rows[0]["cs_buyer_rebate"]);
                BuyerDefaultPremium = Convert.ToDouble(ThisBuyerRecord.Rows[0]["cs_buyer_premium"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Buyer: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Update_Buyer(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;
            Boolean hasChanged = false;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "UPDATE tblBuyers SET ";
                if (BuyerBidderNumber != ThisBuyerRecord.Rows[0]["cs_buyer_bidderno"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_bidderno = '" + MyFormatting.Hyphon(BuyerBidderNumber) + "', ";
                    hasChanged = true;
                }
                if (BuyerUnityCode != ThisBuyerRecord.Rows[0]["cs_buyer_unity"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_unity = '" + MyFormatting.Hyphon(BuyerUnityCode) + "', ";
                    hasChanged = true;
                }
                if (BuyerName1 != ThisBuyerRecord.Rows[0]["cs_buyer_name1"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_name1 = '" + MyFormatting.Hyphon(BuyerName1) + "', ";
                    hasChanged = true;
                }
                if (BuyerName2 != ThisBuyerRecord.Rows[0]["cs_buyer_name2"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_name2 = '" + MyFormatting.Hyphon(BuyerName2) + "', ";
                    hasChanged = true;
                }
                if (BuyerAddress1 != ThisBuyerRecord.Rows[0]["cs_buyer_address1"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_address1 = '" + MyFormatting.Hyphon(BuyerAddress1) + "', ";
                    hasChanged = true;
                }
                if (BuyerAddress2 != ThisBuyerRecord.Rows[0]["cs_buyer_address2"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_address2 = '" + MyFormatting.Hyphon(BuyerAddress2) + "', ";
                    hasChanged = true;
                }
                if (BuyerCity != ThisBuyerRecord.Rows[0]["cs_buyer_city"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_city = '" + MyFormatting.Hyphon(BuyerCity) + "', ";
                    hasChanged = true;
                }
                if (BuyerState != ThisBuyerRecord.Rows[0]["cs_buyer_state"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_state = '" + MyFormatting.Hyphon(BuyerState) + "', ";
                    hasChanged = true;
                }
                if (BuyerPostCode != ThisBuyerRecord.Rows[0]["cs_buyer_postcode"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_postcode = '" + MyFormatting.Hyphon(BuyerPostCode) + "', ";
                    hasChanged = true;
                }
                if (BuyerTelephone != ThisBuyerRecord.Rows[0]["cs_buyer_telephone"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_telephone = '" + MyFormatting.Hyphon(BuyerTelephone) + "', ";
                    hasChanged = true;
                }
                if (BuyerMobile != ThisBuyerRecord.Rows[0]["cs_buyer_mobile"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_mobile = '" + MyFormatting.Hyphon(BuyerMobile) + "', ";
                    hasChanged = true;
                }
                if (BuyerEmail != ThisBuyerRecord.Rows[0]["cs_buyer_email"].ToString())
                {
                    strSQL = strSQL + "cs_buyer_email = '" + MyFormatting.Hyphon(BuyerEmail) + "', ";
                    hasChanged = true;
                }
                if (BuyerDefaultRebate != Convert.ToDouble(ThisBuyerRecord.Rows[0]["cs_buyer_rebate"]))
                {
                    strSQL = strSQL + "cs_buyer_rebate = " + BuyerDefaultRebate.ToString() + ", ";
                    hasChanged = true;
                }
                if (BuyerDefaultPremium != Convert.ToDouble(ThisBuyerRecord.Rows[0]["cs_buyer_premium"]))
                {
                    strSQL = strSQL + "cs_buyer_premium = " + BuyerDefaultPremium.ToString() + ", ";
                    hasChanged = true;
                }

                if (hasChanged == true)
                {
                    strSQL = strSQL.Substring(0, strSQL.Length - 2);
                    strSQL = strSQL + " WHERE cs_buyer_id = " + BuyerId.ToString();
                    SqlCommand cmdUpdate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                    if (cmdUpdate.ExecuteNonQuery() != 1)
                    {
                        isSuccessful = false;
                        ErrorMessage = "** Operator **\r\n\r\nUpdate Buyer Record: More than one record would be updated !";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nUpdate Buyer Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Can_Delete_Buyer(Int32 buyerId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblLots WHERE cs_lot_buyerid = " + buyerId.ToString();
                SqlCommand cmdGetL = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGetL = cmdGetL.ExecuteReader();
                if (rdrGetL.HasRows == true)
                {
                    isSuccessful = false;
                    ErrorMessage = ErrorMessage + "There are Auction Lots associated with this Buyer !\r\n";
                }
                rdrGetL.Close();

                if (isSuccessful == false)
                {
                    ErrorMessage = "** Operator **\r\n\r\nTest if Buyer can be Deleted:\r\n\r\n" + ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nTest if Buyer can be Deleted: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Buyer(SqlTransaction trnEnvelope, Int32 buyerId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblBuyers WHERE cs_buyer_id = " + buyerId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Buyer Record: More than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Buyer Record: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Int32 Buyers_In_Sale()
        {
            Int32 buyerCount = 0;
            DataTable myBuyers = new DataTable();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers ORDER BY cs_buyer_name1";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    myBuyers.Load(rdrGet);
                    buyerCount = myBuyers.Rows.Count;
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                buyerCount = -1;
                ErrorMessage = "** Operator **\r\n\r\nNumber of Buyers in Sale: " + ex.Message + " !";
            }

            return buyerCount;
        }
        public Boolean Get_List_Of_Buyers()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            BuyerRecords.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblBuyers ORDER BY cs_buyer_name1";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    BuyerRecords.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet List of Buyers: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region Lot Table
        public Int32 LotId { get; set; }
        public String LotNumber { get; set; }
        public Int32 LotVendorId { get; set; }
        public Int32 LotCatalogueId { get; set; }
        public String LotDescription { get; set; }
        public Double LotQuantity { get; set; }
        public Double LotPrice { get; set; }
        public Double LotValue { get; set; }
        public String LotGSTCode { get; set; }
        public Double LotCommission { get; set; }
        public Int32 LotBuyerid { get; set; }
        public Double LotRebate { get; set; }
        public Double LotPremium { get; set; }
        public Int32 LotInvoiceNumber { get; set; }
        public Int32 LotAccountSaleNumber { get; set; }

        public DataTable ThisLotRecord { get; set; } = new DataTable();
        public DataTable LotRecords { get; set; } = new DataTable();

        public Boolean Create_Lots_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblLots (";
                strSQL = strSQL + "cs_lot_id Bigint NOT NULL IDENTITY, ";
                strSQL = strSQL + "cs_lot_vendorid Bigint NOT NULL, ";
                strSQL = strSQL + "cs_lot_buyerid Bigint NOT NULL, ";
                strSQL = strSQL + "cs_lot_catalogueid Bigint NOT NULL, ";
                strSQL = strSQL + "cs_lot_number nvarchar(10) NOT NULL, ";
                strSQL = strSQL + "cs_lot_description nvarchar(256) NOT NULL, ";
                strSQL = strSQL + "cs_lot_quantity float NOT NULL, ";
                strSQL = strSQL + "cs_lot_price float NOT NULL, ";
                strSQL = strSQL + "cs_lot_value float NOT NULL, ";
                strSQL = strSQL + "cs_lot_gstcode char(1) NOT NULL, ";
                strSQL = strSQL + "cs_lot_commissionrate float NOT NULL, ";
                strSQL = strSQL + "cs_lot_rebaterate float NOT NULL, ";
                strSQL = strSQL + "cs_lot_premiumrate float NOT NULL, ";
                strSQL = strSQL + "cs_lot_invoiceno Bigint NOT NULL, ";
                strSQL = strSQL + "cs_lot_accountsaleno Bigint NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);

                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate Lots Table: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Insert_Lot(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblLots (";
                strSQL = strSQL + "cs_lot_vendorid, ";
                strSQL = strSQL + "cs_lot_buyerid, ";
                strSQL = strSQL + "cs_lot_catalogueid, ";
                strSQL = strSQL + "cs_lot_number, ";
                strSQL = strSQL + "cs_lot_description, ";
                strSQL = strSQL + "cs_lot_quantity, ";
                strSQL = strSQL + "cs_lot_price, ";
                strSQL = strSQL + "cs_lot_value, ";
                strSQL = strSQL + "cs_lot_gstcode, ";
                strSQL = strSQL + "cs_lot_commissionrate, ";
                strSQL = strSQL + "cs_lot_rebaterate, ";
                strSQL = strSQL + "cs_lot_premiumrate, ";
                strSQL = strSQL + "cs_lot_invoiceno, ";
                strSQL = strSQL + "cs_lot_accountsaleno) VALUES (";
                strSQL = strSQL + LotVendorId.ToString() + ", ";
                strSQL = strSQL + LotBuyerid.ToString() + ", ";
                strSQL = strSQL + LotCatalogueId.ToString() + ", ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(LotNumber) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(LotDescription) + "', ";
                strSQL = strSQL + LotQuantity.ToString() + ", ";
                strSQL = strSQL + LotPrice.ToString() + ", ";
                strSQL = strSQL + LotValue.ToString() + ", ";
                strSQL = strSQL + "'" + LotGSTCode + "', ";
                strSQL = strSQL + LotCommission.ToString() + ", ";
                strSQL = strSQL + LotRebate.ToString() + ", ";
                strSQL = strSQL + LotPremium.ToString() + ", ";
                strSQL = strSQL + LotInvoiceNumber.ToString() + ", ";
                strSQL = strSQL + LotAccountSaleNumber.ToString() + ")";
                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert New Lot: More than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert New Lot: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Lot(Int32 lotId)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ThisLotRecord.Clear();

            try
            {
                String strSQL = "SELECT *FROM tblLots WHERE cs_lot_id = " + lotId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisLotRecord.Load(rdrGet);
                    isSuccessful = Gather_Lot();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Lot by Id : Sale Lot with Id " + lotId.ToString() + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Lot by Id : " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_Lot(Int32 lotId, SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            ThisLotRecord.Clear();

            try
            {
                String strSQL = "SELECT *FROM tblLots WHERE cs_lot_id = " + lotId.ToString();
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisLotRecord.Load(rdrGet);
                    isSuccessful = Gather_Lot();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet Lot by Id : Sale Lot with Id " + lotId.ToString() + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet Lot by Id : " + ex.Message + " !";
            }

            return isSuccessful;
        }

        public Boolean Gather_Lot()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                LotAccountSaleNumber = Convert.ToInt32(ThisLotRecord.Rows[0]["cs_lot_accountsaleno"]);
                LotBuyerid = Convert.ToInt32(ThisLotRecord.Rows[0]["cs_lot_buyerid"]);
                LotCatalogueId = Convert.ToInt32(ThisLotRecord.Rows[0]["cs_lot_catalogueid"]);
                LotCommission = Convert.ToDouble(ThisLotRecord.Rows[0]["cs_lot_commissionrate"]);
                LotDescription = ThisLotRecord.Rows[0]["cs_lot_description"].ToString();
                LotGSTCode = ThisLotRecord.Rows[0]["cs_lot_gstcode"].ToString();
                LotId = Convert.ToInt32(ThisLotRecord.Rows[0]["cs_lot_id"]);
                LotInvoiceNumber = Convert.ToInt32(ThisLotRecord.Rows[0]["cs_lot_invoiceno"]);
                LotNumber = ThisLotRecord.Rows[0]["cs_lot_number"].ToString();
                LotPremium = Convert.ToDouble(ThisLotRecord.Rows[0]["cs_lot_premiumrate"]);
                LotPrice = Convert.ToDouble(ThisLotRecord.Rows[0]["cs_lot_price"]);
                LotQuantity = Convert.ToDouble(ThisLotRecord.Rows[0]["cs_lot_quantity"]);
                LotRebate = Convert.ToDouble(ThisLotRecord.Rows[0]["cs_lot_rebaterate"]);
                LotValue = Convert.ToDouble(ThisLotRecord.Rows[0]["cs_lot_value"]);
                LotVendorId = Convert.ToInt32(ThisLotRecord.Rows[0]["cs_lot_vendorid"]);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather Lot: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Delete_Lot(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblLots WHERE cs_lot_id = " + LotId.ToString();
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdDelete.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nDelete Lot: More than one record would be deleted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nDelete Lot: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Int32 Get_Lot_Count()
        {
            Int32 lotCount = 0; ;

            ErrorMessage = string.Empty;

            if (Get_List_Of_Lots("") == true)
            {
                lotCount = LotRecords.Rows.Count;
            }

            return lotCount;
        }
        public Boolean Get_List_Of_Lots(String myOrder)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            LotRecords.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblLots ";
                strSQL = strSQL + "INNER JOIN tblVendors ON tblLots.cs_lot_vendorid = tblVendors.cs_vendor_id ";
                strSQL = strSQL + "INNER JOIN tblBuyers ON tblLots.cs_lot_buyerid = tblBuyers.cs_buyer_id ";
                if (myOrder == "B")
                    strSQL = strSQL + "ORDER BY tblBuyers.cs_buyer_bidderno, tblLots.cs_lot_number";
                else if (myOrder == "V")
                    strSQL = strSQL + "ORDER BY tblVendors.cs_vendor_id, tblLots.cs_lot_number";
                else
                    strSQL = strSQL + "ORDER BY tblLots.cs_lot_number";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    LotRecords.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet List of Lots: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        #endregion
        #region UNITYClients
        public Int32 UNITYId { get; set; }
        public String UNITYShortName { get; set; }
        public String UNITYName1 { get; set; }
        public String UNITYName2 { get; set; }
        public String UNITYProperty { get; set; }
        public String UNITYStreet { get; set; }
        public String UNITYCity { get; set; }
        public String UNITYState { get; set; }
        public String UNITYPostCode { get; set; }
        public String UNITYTelephone { get; set; }
        public String UNITYFax { get; set; }
        public String UNITYMobile { get; set; }
        public String UNITYEmail { get; set; }
        public String UNITYPIC { get; set; }
        public String UNITYABN { get; set; }
        public Boolean UNITYGSTStatus { get; set; }
        public String UNITYMailingStreet { get; set; }
        public String UNITYMailingCity { get; set; }
        public String UNITYMailingState { get; set; }
        public String UNITYMailingPostCode { get; set; }
        public DataTable ThisUNITYClient { get; set; } = new DataTable();
        public DataTable UNITYClients { get; set; } = new DataTable();

        public Boolean UNITY_Client_Table_Exists()
        {
            Boolean tableExists = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "SELECT * FROM tblUNITYClients";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                tableExists = false;
                ErrorMessage = ex.Message;
            }

            return tableExists;
        }
        public Boolean Create_UNITY_Client_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "CREATE TABLE tblUNITYClients (";
                strSQL = strSQL + "cs_unity_id Bigint NOT NULL IDENTITY, ";
                strSQL = strSQL + "cs_unity_sname nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_unity_name1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_name2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_property nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_street nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_city nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_state nvarchar(3) NOT NULL, ";
                strSQL = strSQL + "cs_unity_postcode nvarchar(5) NOT NULL, ";
                strSQL = strSQL + "cs_unity_phone nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_unity_tax nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_unity_mobile nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_unity_email nvarchar(128) NOT NULL, ";
                strSQL = strSQL + "cs_unity_pic nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_unity_abn nvarchar(15) NOT NULL, ";
                strSQL = strSQL + "cs_unity_gst bit NOT NULL, ";
                strSQL = strSQL + "cs_unity_mail1 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_mail2 nvarchar(50) NOT NULL, ";
                strSQL = strSQL + "cs_unity_mail3 nvarchar(3) NOT NULL, ";
                strSQL = strSQL + "cs_unity_mail4 nvarchar(5) NOT NULL)";
                SqlCommand cmdCreate = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nCreate UNITY Client Table: " + ex.Message + " !";
            }

            return isSuccessful;
        }

        public Boolean Insert_UNITY_Client(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "INSERT INTO tblUNITYClients (";
                strSQL = strSQL + "cs_unity_sname, ";
                strSQL = strSQL + "cs_unity_name1, ";
                strSQL = strSQL + "cs_unity_name2, ";
                strSQL = strSQL + "cs_unity_property, ";
                strSQL = strSQL + "cs_unity_street, ";
                strSQL = strSQL + "cs_unity_city, ";
                strSQL = strSQL + "cs_unity_state, ";
                strSQL = strSQL + "cs_unity_postcode, ";
                strSQL = strSQL + "cs_unity_phone, ";
                strSQL = strSQL + "cs_unity_tax, ";
                strSQL = strSQL + "cs_unity_mobile, ";
                strSQL = strSQL + "cs_unity_email, ";
                strSQL = strSQL + "cs_unity_pic, ";
                strSQL = strSQL + "cs_unity_abn, ";
                strSQL = strSQL + "cs_unity_gst, ";
                strSQL = strSQL + "cs_unity_mail1, ";
                strSQL = strSQL + "cs_unity_mail2, ";
                strSQL = strSQL + "cs_unity_mail3, ";
                strSQL = strSQL + "cs_unity_mail4) VALUES (";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYShortName) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYName1) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYName2) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYProperty) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYStreet) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYCity) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYState) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYPostCode) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYTelephone) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYFax) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYMobile) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYEmail) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYPIC) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYABN) + "', ";
                strSQL = strSQL + "'" + UNITYGSTStatus.ToString() + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYMailingStreet) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYMailingCity) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYMailingState) + "', ";
                strSQL = strSQL + "'" + MyFormatting.Hyphon(UNITYMailingPostCode) + "')";

                SqlCommand cmdInsert = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                if (cmdInsert.ExecuteNonQuery() != 1)
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nInsert UNITY Client: More than one record would be inserted !";
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nInsert UNITY Client: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Get_UNITY_Client(String shortName)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            ThisUNITYClient.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblUNITYClients WHERE cs_unity_sname = '" + MyFormatting.Hyphon(shortName) + "'";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    ThisUNITYClient.Load(rdrGet);
                    isSuccessful = Gather_UNITY_Client();
                }
                else
                {
                    isSuccessful = false;
                    ErrorMessage = "** Operator **\r\n\r\nGet UNITY Client: Client with Short Name " + shortName + " not found !";
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet UNITY Client: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Gather_UNITY_Client()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                UNITYABN = ThisUNITYClient.Rows[0]["cs_unity_abn"].ToString();
                UNITYCity = ThisUNITYClient.Rows[0]["cs_unity_city"].ToString();
                UNITYEmail = ThisUNITYClient.Rows[0]["cs_unity_email"].ToString();
                UNITYFax = ThisUNITYClient.Rows[0]["cs_unity_tax"].ToString();
                UNITYGSTStatus = Convert.ToBoolean(ThisUNITYClient.Rows[0]["cs_unity_gst"]);
                UNITYMailingCity = ThisUNITYClient.Rows[0]["cs_unity_mail2"].ToString();
                UNITYMailingPostCode = ThisUNITYClient.Rows[0]["cs_unity_mail4"].ToString();
                UNITYMailingState = ThisUNITYClient.Rows[0]["cs_unity_mail3"].ToString();
                UNITYMailingStreet = ThisUNITYClient.Rows[0]["cs_unity_mail1"].ToString();
                UNITYMobile = ThisUNITYClient.Rows[0]["cs_unity_mobile"].ToString();
                UNITYName1 = ThisUNITYClient.Rows[0]["cs_unity_name1"].ToString();
                UNITYName2 = ThisUNITYClient.Rows[0]["cs_unity_name2"].ToString();
                UNITYPIC = ThisUNITYClient.Rows[0]["cs_unity_pic"].ToString();
                UNITYPostCode = ThisUNITYClient.Rows[0]["cs_unity_postcode"].ToString();
                UNITYProperty = ThisUNITYClient.Rows[0]["cs_unity_property"].ToString();
                UNITYShortName = ThisUNITYClient.Rows[0]["cs_unity_sname"].ToString();
                UNITYState = ThisUNITYClient.Rows[0]["cs_unity_state"].ToString();
                UNITYStreet = ThisUNITYClient.Rows[0]["cs_unity_street"].ToString();
                UNITYTelephone = ThisUNITYClient.Rows[0]["cs_unity_phone"].ToString();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGather UNITY Client: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Boolean Clear_UNITY_Client_Table(SqlTransaction trnEnvelope)
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;

            try
            {
                String strSQL = "DELETE FROM tblUNITYClients";
                SqlCommand cmdDelete = new SqlCommand(strSQL, MyConnection, trnEnvelope);
                cmdDelete.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nClear UNITY Client Table: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public Int32 Get_UNITY_Client_Count()
        {
            Int32 clientCount = 0;

            if (Get_UNITY_Clients() == true)
                clientCount = UNITYClients.Rows.Count;

            return clientCount;
        }
        public Boolean Get_UNITY_Clients()
        {
            Boolean isSuccessful = true;

            ErrorMessage = string.Empty;
            UNITYClients.Clear();

            try
            {
                String strSQL = "SELECT * FROM tblUNITYClients ORDER BY cs_unity_sname";
                SqlCommand cmdGet = new SqlCommand(strSQL, MyConnection);
                SqlDataReader rdrGet = cmdGet.ExecuteReader();
                if (rdrGet.HasRows == true)
                {
                    UNITYClients.Load(rdrGet);
                }
                rdrGet.Close();
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                ErrorMessage = "** Operator **\r\n\r\nGet UNITY Clients: " + ex.Message + " !";
            }

            return isSuccessful;
        }
        public String Browse_UNITY_clients(String shortName)
        {
            String thisClient = string.Empty;

            ErrorMessage = string.Empty;

            if (Get_UNITY_Clients() == true)
            {
                FrmUNITYBrowse clientBrowse = new FrmUNITYBrowse();
                clientBrowse.myClients = UNITYClients;
                clientBrowse.searchName = shortName;
                
                if (clientBrowse.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                {
                    thisClient = clientBrowse.searchName;
                }
                clientBrowse.Close();
            }

            return thisClient;
        }
        #endregion
    }
}
