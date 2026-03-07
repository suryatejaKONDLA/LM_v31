export interface LoginMasterResponse
{
    Login_ID: number;
    Login_User: string;
    Login_Name: string;
    Login_Branch_Code: number;
    Login_Designation: string;
    Login_Mobile_No: string;
    Login_Email_ID: string;
    Login_DOB: string | null;
    Login_Gender: string;
    Login_Active_Flag: boolean;
    Login_Created_Name: string | null;
    Login_Created_Date: string | null;
    Login_Modified_Name: string | null;
    Login_Modified_Date: string | null;
    Login_Approved_Name: string | null;
    Login_Approved_Date: string | null;
}

export interface LoginMasterFormValues
{
    Login_User: string;
    Login_Name: string;
    Login_Designation: string;
    Login_Mobile_No: string;
    Login_Email_ID: string;
    Login_DOB: string | null;
    Login_Gender: string;
    Login_Active_Flag: boolean;
}
