dmc sign 
-f "path to the file to sign" (required:true)
-c "certificate friendly name" (required:true)
--path "path to the certificate file to use" (required:false)
--pwd "password that protects the certificate file" (required:false)

dmc sign -f "C:\User\p1XY-1a.stp" -c "TomH"

dmc verify 
-f "path to the file to verify" (required:true)

dmc verify -f "C:\User\p1XY-1a.stp"

dmc signm 
-f "path to the file to sign" (required:true)
-c "certificate friendly name" (required:true)
--path "path to the certificate file to use" (required:false)
--pwd "password that protects the certificate file" (required:false)
--metadata "key:value" (required:true)

dmc signm -f "C:\User\p1XY-1a.stp" -c "TomH" --metadata "stage:design":"designer:TomH" 