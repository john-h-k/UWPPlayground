using System.Collections.Generic;

namespace UWPPlayground.Common
{
    public static class Errors
    {
        public static IDictionary<int, string> ErrorMap;

        static Errors()
        {
            ErrorMap = new Dictionary<int, string>
            {
                #region FACILITY_* Constants

                [0] = "FACILITY_NULL",

                [7] = "FACILITY_WIN32",

                [2170] = "FACILITY_DXGI",

                [2174] = "FACILITY_DIRECT3D12",

                [2175] = "FACILITY_DIRECT3D12_DEBUG",

                [2200] = "FACILITY_WINCODEC_DWRITE_DWM",

                [2201] = "FACILITY_DIRECT2D",

                #endregion

                #region ERROR_* Constants

                [2] = "ERROR_FILE_NOT_FOUND",

                [5] = "ERROR_ACCESS_DENIED",

                [6] = "ERROR_INVALID_HANDLE",

                [14] = "ERROR_OUTOFMEMORY",

                [87] = "ERROR_INVALID_PARAMETER",

                [122] = "ERROR_INSUFFICIENT_BUFFER",

                [534] = "ERROR_ARITHMETIC_OVERFLOW",

                #endregion

                #region E_* Constants

                [unchecked((int) 0x8000FFFF)] = "E_UNEXPECTED",

                [unchecked((int) 0x80004001)] = "E_NOTIMPL",

                [unchecked((int) 0x8007000E)] = "E_OUTOFMEMORY",

                [unchecked((int) 0x80070057)] = "E_INVALIDARG",

                [unchecked((int) 0x80004002)] = "E_NOINTERFACE",

                [unchecked((int) 0x80004003)] = "E_POINTER",

                [unchecked((int) 0x80070006)] = "E_HANDLE",

                [unchecked((int) 0x80004004)] = "E_ABORT",

                [unchecked((int) 0x80004005)] = "E_FAIL",

                [unchecked((int) 0x80070005)] = "E_ACCESSDENIED",

                #endregion

                #region DXGI_STATUS_* Constants

                [0x087A0001] = "DXGI_STATUS_OCCLUDED",

                [0x087A0002] = "DXGI_STATUS_CLIPPED",

                [0x087A0004] = "DXGI_STATUS_NO_REDIRECTION",

                [0x087A0005] = "DXGI_STATUS_NO_DESKTOP_ACCESS",

                [0x087A0006] = "DXGI_STATUS_GRAPHICS_VIDPN_SOURCE_IN_USE",

                [0x087A0007] = "DXGI_STATUS_MODE_CHANGED",

                [0x087A0008] = "DXGI_STATUS_MODE_CHANGE_IN_PROGRESS",

                [0x087A0009] = "DXGI_STATUS_UNOCCLUDED",

                [0x087A000A] = "DXGI_STATUS_DDA_WAS_STILL_DRAWING",

                [0x087A002F] = "DXGI_STATUS_PRESENT_REQUIRED",

                #endregion

                #region DXGI_ERROR_* Constants

                [unchecked((int) 0x887A0001)] = "DXGI_ERROR_INVALID_CALL",

                [unchecked((int) 0x887A0002)] = "DXGI_ERROR_NOT_FOUND",

                [unchecked((int) 0x887A0003)] = "DXGI_ERROR_MORE_DATA",

                [unchecked((int) 0x887A0004)] = "DXGI_ERROR_UNSUPPORTED",

                [unchecked((int) 0x887A0005)] = "DXGI_ERROR_DEVICE_REMOVED",

                [unchecked((int) 0x887A0006)] = "DXGI_ERROR_DEVICE_HUNG",

                [unchecked((int) 0x887A0007)] = "DXGI_ERROR_DEVICE_RESET",

                [unchecked((int) 0x887A000A)] = "DXGI_ERROR_WAS_STILL_DRAWING",

                [unchecked((int) 0x887A000B)] = "DXGI_ERROR_FRAME_STATISTICS_DISJOINT",

                [unchecked((int) 0x887A000C)] = "DXGI_ERROR_GRAPHICS_VIDPN_SOURCE_IN_USE",

                [unchecked((int) 0x887A0020)] = "DXGI_ERROR_DRIVER_INTERNAL_ERROR",

                [unchecked((int) 0x887A0021)] = "DXGI_ERROR_NONEXCLUSIVE",

                [unchecked((int) 0x887A0022)] = "DXGI_ERROR_NOT_CURRENTLY_AVAILABLE",

                [unchecked((int) 0x887A0023)] = "DXGI_ERROR_REMOTE_CLIENT_DISCONNECTED",

                [unchecked((int) 0x887A0024)] = "DXGI_ERROR_REMOTE_OUTOFMEMORY",

                [unchecked((int) 0x887A0026)] = "DXGI_ERROR_ACCESS_LOST",

                [unchecked((int) 0x887A0027)] = "DXGI_ERROR_WAIT_TIMEOUT",

                [unchecked((int) 0x887A0028)] = "DXGI_ERROR_SESSION_DISCONNECTED",

                [unchecked((int) 0x887A0029)] = "DXGI_ERROR_RESTRICT_TO_OUTPUT_STALE",

                [unchecked((int) 0x887A002A)] = "DXGI_ERROR_CANNOT_PROTECT_CONTENT",

                [unchecked((int) 0x887A002B)] = "DXGI_ERROR_ACCESS_DENIED",

                [unchecked((int) 0x887A002C)] = "DXGI_ERROR_NAME_ALREADY_EXISTS",

                [unchecked((int) 0x887A002D)] = "DXGI_ERROR_SDK_COMPONENT_MISSING",

                [unchecked((int) 0x887A002E)] = "DXGI_ERROR_NOT_CURRENT",

                [unchecked((int) 0x887A0030)] = "DXGI_ERROR_HW_PROTECTION_OUTOFMEMORY",

                [unchecked((int) 0x887A0031)] = "DXGI_ERROR_DYNAMIC_CODE_POLICY_VIOLATION",

                [unchecked((int) 0x887A0032)] = "DXGI_ERROR_NON_COMPOSITED_UI",

                [unchecked((int) 0x887A0025)] = "DXGI_ERROR_MODE_CHANGE_IN_PROGRESS",

                [unchecked((int) 0x887A0033)] = "DXGI_ERROR_CACHE_CORRUPT",

                [unchecked((int) 0x887A0034)] = "DXGI_ERROR_CACHE_FULL",

                [unchecked((int) 0x887A0035)] = "DXGI_ERROR_CACHE_HASH_COLLISION",

                [unchecked((int) 0x887A0036)] = "DXGI_ERROR_ALREADY_EXISTS",

                #endregion

                #region D3D12_ERROR_* Constants

                [unchecked((int) 0x887E0001)] = "D3D12_ERROR_ADAPTER_NOT_FOUND",

                [unchecked((int) 0x887E0002)] = "D3D12_ERROR_DRIVER_VERSION_MISMATCH",

                #endregion

                #region D2DERR_* Constants

                [unchecked((int) 0x88990001)] = "D2DERR_WRONG_STATE",

                [unchecked((int) 0x88990002)] = "D2DERR_NOT_INITIALIZED",

                [unchecked((int) 0x88990003)] = "D2DERR_UNSUPPORTED_OPERATION",

                [unchecked((int) 0x88990004)] = "D2DERR_SCANNER_FAILED",

                [unchecked((int) 0x88990005)] = "D2DERR_SCREEN_ACCESS_DENIED",

                [unchecked((int) 0x88990006)] = "D2DERR_DISPLAY_STATE_INVALID",

                [unchecked((int) 0x88990007)] = "D2DERR_ZERO_VECTOR",

                [unchecked((int) 0x88990008)] = "D2DERR_INTERNAL_ERROR",

                [unchecked((int) 0x88990009)] = "D2DERR_DISPLAY_FORMAT_NOT_SUPPORTED",

                [unchecked((int) 0x8899000A)] = "D2DERR_INVALID_CALL",

                [unchecked((int) 0x8899000B)] = "D2DERR_NO_HARDWARE_DEVICE",

                [unchecked((int) 0x8899000C)] = "D2DERR_RECREATE_TARGET",

                [unchecked((int) 0x8899000D)] = "D2DERR_TOO_MANY_SHADER_ELEMENTS",

                [unchecked((int) 0x8899000E)] = "D2DERR_SHADER_COMPILE_FAILED",

                [unchecked((int) 0x8899000F)] = "D2DERR_MAX_TEXTURE_SIZE_EXCEEDED",

                [unchecked((int) 0x88990010)] = "D2DERR_UNSUPPORTED_VERSION",

                [unchecked((int) 0x88990011)] = "D2DERR_BAD_NUMBER",

                [unchecked((int) 0x88990012)] = "D2DERR_WRONG_FACTORY",

                [unchecked((int) 0x88990013)] = "D2DERR_LAYER_ALREADY_IN_USE",

                [unchecked((int) 0x88990014)] = "D2DERR_POP_CALL_DID_NOT_MATCH_PUSH",

                [unchecked((int) 0x88990015)] = "D2DERR_WRONG_RESOURCE_DOMAIN",

                [unchecked((int) 0x88990016)] = "D2DERR_PUSH_POP_UNBALANCED",

                [unchecked((int) 0x88990017)] = "D2DERR_RENDER_TARGET_HAS_LAYER_OR_CLIPRECT",

                [unchecked((int) 0x88990018)] = "D2DERR_INCOMPATIBLE_BRUSH_TYPES",

                [unchecked((int) 0x88990019)] = "D2DERR_WIN32_ERROR",

                [unchecked((int) 0x8899001A)] = "D2DERR_TARGET_NOT_GDI_COMPATIBLE",

                [unchecked((int) 0x8899001B)] = "D2DERR_TEXT_EFFECT_IS_WRONG_TYPE",

                [unchecked((int) 0x8899001C)] = "D2DERR_TEXT_RENDERER_NOT_RELEASED",

                [unchecked((int) 0x8899001D)] = "D2DERR_EXCEEDS_MAX_BITMAP_SIZE",

                [unchecked((int) 0x8899001E)] = "D2DERR_INVALID_GRAPH_CONFIGURATION",

                [unchecked((int) 0x8899001F)] = "D2DERR_INVALID_INTERNAL_GRAPH_CONFIGURATION",

                [unchecked((int) 0x88990020)] = "D2DERR_CYCLIC_GRAPH",

                [unchecked((int) 0x88990021)] = "D2DERR_BITMAP_CANNOT_DRAW",

                [unchecked((int) 0x88990022)] = "D2DERR_OUTSTANDING_BITMAP_REFERENCES",

                [unchecked((int) 0x88990023)] = "D2DERR_ORIGINAL_TARGET_NOT_BOUND",

                [unchecked((int) 0x88990024)] = "D2DERR_INVALID_TARGET",

                [unchecked((int) 0x88990025)] = "D2DERR_BITMAP_BOUND_AS_TARGET",

                [unchecked((int) 0x88990026)] = "D2DERR_INSUFFICIENT_DEVICE_CAPABILITIES",

                [unchecked((int) 0x88990027)] = "D2DERR_INTERMEDIATE_TOO_LARGE",

                [unchecked((int) 0x88990028)] = "D2DERR_EFFECT_IS_NOT_REGISTERED",

                [unchecked((int) 0x88990029)] = "D2DERR_INVALID_PROPERTY",

                [unchecked((int) 0x8899002A)] = "D2DERR_NO_SUBPROPERTIES",

                [unchecked((int) 0x8899002B)] = "D2DERR_PRINT_JOB_CLOSED",

                [unchecked((int) 0x8899002C)] = "D2DERR_PRINT_FORMAT_NOT_SUPPORTED",

                [unchecked((int) 0x8899002D)] = "D2DERR_TOO_MANY_TRANSFORM_INPUTS",

                [unchecked((int) 0x8899002E)] = "D2DERR_INVALID_GLYPH_IMAGE",

                #endregion

                #region WINCODEC_ERR_* Constants

                [unchecked((int) 0x88982F04)] = "WINCODEC_ERR_WRONGSTATE",

                [unchecked((int) 0x88982F05)] = "WINCODEC_ERR_VALUEOUTOFRANGE",

                [unchecked((int) 0x88982F07)] = "WINCODEC_ERR_UNKNOWNIMAGEFORMAT",

                [unchecked((int) 0x88982F0B)] = "WINCODEC_ERR_UNSUPPORTEDVERSION",

                [unchecked((int) 0x88982F0C)] = "WINCODEC_ERR_NOTINITIALIZED",

                [unchecked((int) 0x88982F0D)] = "WINCODEC_ERR_ALREADYLOCKED",

                [unchecked((int) 0x88982F40)] = "WINCODEC_ERR_PROPERTYNOTFOUND",

                [unchecked((int) 0x88982F41)] = "WINCODEC_ERR_PROPERTYNOTSUPPORTED",

                [unchecked((int) 0x88982F42)] = "WINCODEC_ERR_PROPERTYSIZE",

                [unchecked((int) 0x88982F43)] = "WINCODEC_ERR_CODECPRESENT",

                [unchecked((int) 0x88982F44)] = "WINCODEC_ERR_CODECNOTHUMBNAIL",

                [unchecked((int) 0x88982F45)] = "WINCODEC_ERR_PALETTEUNAVAILABLE",

                [unchecked((int) 0x88982F46)] = "WINCODEC_ERR_CODECTOOMANYSCANLINES",

                [unchecked((int) 0x88982F48)] = "WINCODEC_ERR_INTERNALERROR",

                [unchecked((int) 0x88982F49)] = "WINCODEC_ERR_SOURCERECTDOESNOTMATCHDIMENSIONS",

                [unchecked((int) 0x88982F50)] = "WINCODEC_ERR_COMPONENTNOTFOUND",

                [unchecked((int) 0x88982F51)] = "WINCODEC_ERR_IMAGESIZEOUTOFRANGE",

                [unchecked((int) 0x88982F52)] = "WINCODEC_ERR_TOOMUCHMETADATA",

                [unchecked((int) 0x88982F60)] = "WINCODEC_ERR_BADIMAGE",

                [unchecked((int) 0x88982F61)] = "WINCODEC_ERR_BADHEADER",

                [unchecked((int) 0x88982F62)] = "WINCODEC_ERR_FRAMEMISSING",

                [unchecked((int) 0x88982F63)] = "WINCODEC_ERR_BADMETADATAHEADER",

                [unchecked((int) 0x88982F70)] = "WINCODEC_ERR_BADSTREAMDATA",

                [unchecked((int) 0x88982F71)] = "WINCODEC_ERR_STREAMWRITE",

                [unchecked((int) 0x88982F72)] = "WINCODEC_ERR_STREAMREAD",

                [unchecked((int) 0x88982F73)] = "WINCODEC_ERR_STREAMNOTAVAILABLE",

                [unchecked((int) 0x88982F80)] = "WINCODEC_ERR_UNSUPPORTEDPIXELFORMAT",

                [unchecked((int) 0x88982F81)] = "WINCODEC_ERR_UNSUPPORTEDOPERATION",

                [unchecked((int) 0x88982F8A)] = "WINCODEC_ERR_INVALIDREGISTRATION",

                [unchecked((int) 0x88982F8B)] = "WINCODEC_ERR_COMPONENTINITIALIZEFAILURE",

                [unchecked((int) 0x88982F8C)] = "WINCODEC_ERR_INSUFFICIENTBUFFER",

                [unchecked((int) 0x88982F8D)] = "WINCODEC_ERR_DUPLICATEMETADATAPRESENT",

                [unchecked((int) 0x88982F8E)] = "WINCODEC_ERR_PROPERTYUNEXPECTEDTYPE",

                [unchecked((int) 0x88982F8F)] = "WINCODEC_ERR_UNEXPECTEDSIZE",

                [unchecked((int) 0x88982F90)] = "WINCODEC_ERR_INVALIDQUERYREQUEST",

                [unchecked((int) 0x88982F91)] = "WINCODEC_ERR_UNEXPECTEDMETADATATYPE",

                [unchecked((int) 0x88982F92)] = "WINCODEC_ERR_REQUESTONLYVALIDATMETADATAROOT",

                [unchecked((int) 0x88982F93)] = "WINCODEC_ERR_INVALIDQUERYCHARACTER",

                [unchecked((int) 0x88982F94)] = "WINCODEC_ERR_WIN32ERROR",

                [unchecked((int) 0x88982F95)] = "WINCODEC_ERR_INVALIDPROGRESSIVELEVEL",

                [unchecked((int) 0x88982F96)] = "WINCODEC_ERR_INVALIDJPEGSCANINDEX",

                #endregion
            };
        }
}
}