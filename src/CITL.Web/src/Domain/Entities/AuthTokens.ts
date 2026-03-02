import { type BranchInfo } from "./User";

/**
 * Login request payload.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Authentication.LoginRequest.
 */
export interface LoginRequest
{
    Login_User: string;
    Login_Password: string;
    Login_Latitude: number;
    Login_Longitude: number;
    Login_Accuracy: number;
    Login_IP: string;
    Login_Device: string;
    Captcha_Id?: string;
    Captcha_Value?: string;
}

/**
 * Login response from Auth/Login and Auth/Refresh.
 * Properties match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Authentication.LoginResponse.
 */
export interface LoginResponse
{
    Access_Token: string;
    Refresh_Token: string;
    Expires_At_Utc: string;
    Login_Id: number;
    Login_User: string;
    Login_Name: string;
    Roles: string[];
    Branches: BranchInfo[];
    Must_Change_Password: boolean;
}

/**
 * Refresh token request payload.
 * Mirrors CITL.Application.Core.Authentication.RefreshTokenRequest.
 */
export interface RefreshTokenRequest
{
    Refresh_Token: string;
    Login_User: string;
}

/**
 * CAPTCHA generation request.
 * Mirrors CITL.Application.Core.Authentication.CaptchaRequest.
 */
export interface CaptchaRequest
{
    Login_User: string;
}

/**
 * CAPTCHA generation response with dual-theme images.
 * Mirrors CITL.Application.Core.Authentication.CaptchaResponse.
 */
export interface CaptchaResponse
{
    Captcha_Id: string;
    Captcha_Image_Light: string;
    Captcha_Image_Dark: string;
    Captcha_Required: boolean;
    Failed_Attempts: number;
}

/**
 * Forgot password request.
 * Mirrors CITL.Application.Core.Authentication.ForgotPasswordRequest.
 */
export interface ForgotPasswordRequest
{
    Login_User: string;
    Login_Email_ID: string;
    Login_Mobile_No: string;
}

/**
 * Password reset request with token from email link.
 * Mirrors CITL.Application.Core.Authentication.ResetPasswordRequest.
 */
export interface ResetPasswordRequest
{
    Token: string;
    Login_Password: string;
}

/**
 * Resend email verification request.
 * Mirrors CITL.Application.Core.Authentication.ResendVerificationRequest.
 */
export interface ResendVerificationRequest
{
    Login_User: string;
    Login_Email_ID: string;
    Login_Mobile_No: string;
}

/**
 * Email verification request with token from email link.
 * Mirrors CITL.Application.Core.Authentication.VerifyEmailRequest.
 */
export interface VerifyEmailRequest
{
    Token: string;
}
