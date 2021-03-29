@echo off
echo Generating LUIS models...
echo(

call bf luis:generate:cs -f -i "./ComplexStatementParsing/ComplexStatementParsing.json" -o "./ComplexStatementParsing/ComplexModel.cs" --className="Chatbot.CognitiveModels.ComplexModel"
echo(

call bf luis:generate:cs -f -i "./SimpleStatementParsing/SimpleStatementParsing.json" -o "./SimpleStatementParsing/SimpleModel.cs" --className="Chatbot.CognitiveModels.SimpleModel"
echo(

pause