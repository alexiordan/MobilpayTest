# MobilpayTest
Test de utilizare a bibliotecii pentru plati MobilPay

Trebuie sa aveti instalat SDK pentru ASP.Net Core 2.1 de aici: https://www.microsoft.com/net/download/dotnet-core/2.1

Pentru testare am gandit urmatorul scenariu:
1. Activati exceptiile
2. Porniti solutia
3. Pentru criptare apelati: https://localhost:5001/api/payment/
4. ~~Pentru decriptare apelati POST la adresa: https://localhost:5001/api/payment/confirmation~~

In urma apelurilor ar trebui sa apara exceptia: `"System.NullReferenceException: 'Object reference not set to an instance of an object.'
"`

Pentru partea de criptare am gasit o solutie in metoda `Helper.EncryptWithCng`. 

~~Penru decriptare insa nu am gasit inca ceva.~~ La decriptare problema era desincronizarea cheii private.
