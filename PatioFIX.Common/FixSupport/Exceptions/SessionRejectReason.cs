namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// Code to identify reason for a session-level Reject <3> message. 
    /// </summary>
    public enum SessionRejectReason : int
    {
        NotSet = -1,
        /// <summary>
        /// 
        /// </summary>
        Invalid_tag_number = 0,
        /// <summary>
        /// 
        /// </summary>
        Required_tag_missing = 1,
        /// <summary>
        /// 
        /// </summary>
        Tag_not_defined_for_this_message_type = 2,
        /// <summary>
        /// 
        /// </summary>
        Undefined_Tag = 3,
        /// <summary>
        /// 
        /// </summary>
        Tag_specified_without_a_value = 4,
        /// <summary>
        /// 
        /// </summary>
        Value_is_incorrect_out_of_range_for_this_tag = 5,
        /// <summary>
        /// 
        /// </summary>
        Incorrect_data_format_for_value = 6,
        /// <summary>
        /// 
        /// </summary>
        Decryption_problem = 7,
        /// <summary>
        /// 
        /// </summary>
        Signature_89_problem = 8,
        /// <summary>
        /// 
        /// </summary>
        CompID_problem = 9,
        /// <summary>
        /// 
        /// </summary>
        SendingTime_52_accuracy_problem = 10,
        /// <summary>
        /// 
        /// </summary>
        Invalid_MsgType = 11,
        /// <summary>
        /// 
        /// </summary>
        XML_Validation_error = 12,
        /// <summary>
        /// 
        /// </summary>
        Tag_appears_more_than_once = 13,
        /// <summary>
        /// 
        /// </summary>
        Tag_specified_out_of_required_order = 14,
        /// <summary>
        /// 
        /// </summary>
        Repeating_group_fields_out_of_order = 15,
        /// <summary>
        /// 
        /// </summary>
        Incorrect_NumInGroup_count_for_repeating_group = 16,
        /// <summary>
        /// 
        /// </summary>
        Non_Data_value_includes_field_delimiter = 17,
        /// <summary>
        /// 
        /// </summary>
        Other = 99
    }
}
