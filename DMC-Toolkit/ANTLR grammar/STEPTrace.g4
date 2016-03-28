

grammar STEPTrace;

traceblocks: (signature_section NEWLINE)+;
signature_section: TRACE_LABEL id NEWLINE trace_section NEWLINE signature NEWLINE ENDSEC;
id: POUND entity_id ;
entity_id: INT;
trace_section: entity_instance_name ' = PKCS_TRACE(' metadata RPAR;
entity_instance_name: id;
metadata:LBRACE  list_of_fields  RBRACE ;
list_of_fields: field (COMMA field)*;
field: field_name ':' field_value;
field_name: STRING_ID;
field_value: STRING ;
signature: entity_instance_name pkcs_token;
pkcs_token: PKCS pkcs_signature COMMA CROSS_BOOL COMMA cross_index RPAR;
pkcs_signature: STRING;
cross_index: '[' (list_of_trace_ids)? ']';
list_of_trace_ids: entity_instance_name (COMMA entity_instance_name)*;
CROSS_BOOL: 'Y'|'N';
NEWLINE : [\r\n]+ ;
APOSTROPHE: '\'';
STRING: '\'' ('a'..'z' | 'A'..'Z' | '0'..'9' | ':' | '.' | '&' | '/' | '\\' | ';'| '-'| '_')* '\'';
STRING_ID: ('a'..'z' | 'A'..'Z'  |'-'| '_')*;
INT     : [0-9]+;
LBRACE: '{';
RBRACE: '}';
LPAR: '(';
RPAR:')';
PKCS: ' = PKCS'LPAR;
POUND: '#';
COMMA: ', ';
ENDSEC: 'ENDSEC;';
TRACE_LABEL: 'TRACE:';
