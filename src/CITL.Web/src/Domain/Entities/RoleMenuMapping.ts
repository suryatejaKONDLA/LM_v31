export interface RoleMenuMappingRequest {
  roleId: number;
  menuIds: string[];
}

export interface RoleMenuMappingResponse {
  ROLE_ID: number;
  MENU_ID: string;
  ROLE_MENU_Created_ID: number;
  ROLE_MENU_Created_Date: string;
  ROLE_MENU_Modified_ID?: number;
  ROLE_MENU_Modified_Date?: string;
  ROLE_MENU_Approved_ID?: number;
  ROLE_MENU_Approved_Date?: string;
  CreatedByName?: string;
  ModifiedByName?: string;
  ApprovedByName?: string;
}

export interface RoleMenuMappingFormValues {
  roleId: number | null;
  menuIds: string[];
}
