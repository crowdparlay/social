setlocal EnableDelayedExpansion

set dll=%1
set ver=%2
set dst=spec\swagger\%ver%\

mkdir %dst%
swagger tofile --output %dst%\swagger.json        %dll% %ver%
swagger tofile --output %dst%\swagger.yaml --yaml %dll% %ver%