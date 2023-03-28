/*****************************************************************
    Χρηστης Intranet για SQL Server:
*/
    Intranet
    p30vls%)3&vks2#sd


/*****************************************************************UAT*****************************************************************

Session SenderCompID	TargetCompID	Username	Port	DropCopy	SSLEnabled			Password			Που χρησιμοποιείται
----------------------------------------------------------------------------------------------------------------------------------------------
EB_Sender1				ATHEX			EBusername1	8080	FALSE		TRUE				ebF1xUs#R2			IT4Shadow.PatioFixBroker
EB_Sender1				ATHEX			EBusername1	12080	FALSE		TRUE				Password1.

EB_Sender2				ATHEX			EBusername2	8081	FALSE		TRUE				ebF1xUs#R2			DEVPC.PatioFix.Broker
EB_Sender2				ATHEX			EBusername2	12081	FALSE		TRUE				Password1.

EB_Sender3				ATHEX			EBusername3	8082	FALSE		TRUE				ebF1xUs#R2			CatalysShadow
EB_Sender3				ATHEX			EBusername3	12082	FALSE		TRUE				Password1.

EB_Sender4				ATHEX			EBusername4	8083	FALSE		TRUE				ebF1xUs#R2			Skouras
EB_Sender4				ATHEX			EBusername4	12083	FALSE		TRUE				Password1.

EB_Sender5				ATHEX			EBusername5	8084	TRUE		TRUE				ebF1xUs#R2			DEVPC.PatioFixAdmin
EB_Sender5				ATHEX			EBusername5	12084	TRUE		TRUE				Password1.

EB_Sender6				ATHEX			EBusername6	8085	TRUE		TRUE				ebF1xUs#R2			IT4Shadow.PatioFixAdmin
EB_Sender6				ATHEX			EBusername6	12085	TRUE		TRUE				Password1.

EB_Sender7				ATHEX			EBusername7	8086	FALSE		TRUE				Password1.			Horizon
EB_Sender7				ATHEX			EBusername7	12086	FALSE		TRUE				Password1.

EB_Sender8				ATHEX			EBusername8	8087	FALSE		TRUE				ebF1xUs#R8			Daidalos Shadow
EB_Sender8				ATHEX			EBusername8	12087	FALSE		TRUE				Password1.

EB_Sender9				ATHEX			EBusername9	8088	TRUE		TRUE				ebF1xUs#R8			Daidalos Shadow
EB_Sender9				ATHEX			EBusername9	12088	TRUE		TRUE				Password1.



Συνδεομαστε στην ip
			10.200.124.69

Απο το ATHEX με βλεπουν με την ip
			10.67.14.57

το password ειναι κοινο και ειναι το
			Password1.








/*****************************************************************

Get-Help \*-Service
Get-Service -DisplayName "PatioFIX.Admin"

New-Service -Name {PatioFIXAdmin} -BinaryPathName "C:\Program Files\PatioFIXAdmin\PatioFIX.Admin.exe --contentRoot C:\Program Files\PatioFIXAdmin\" -Description "This is the PatioFIX.Admin" -DisplayName "PatioFIX.Admin" -StartupType Automatic
Remove-Service -Name "PatioFIXAdmin" ή
sc.exe delete "PatioFIXAdmin" for PSVersion 5.1



New-Service -Name {PatioFixBroker} -BinaryPathName "C:\Program Files\PatioFixBroker\PatioFix.Broker.exe --contentRoot C:\Program Files\PatioFixBroker\" -Description "This is the PatioFIX.Broker" -DisplayName "PatioFIX.Broker" -StartupType Automatic
Remove-Service -Name "PatioFIXAdmin" ή
sc.exe delete "PatioFIXAdmin" for PSVersion 5.1




New-Service -Name {PatioFIXWatchDog} -BinaryPathName "C:\Program Files\PatioFIXWatchDog\PatioFIX.WatchDog.exe --contentRoot C:\Program Files\PatioFIXWatchDog\" -Description "This is the PatioFIX WatchDog" -DisplayName "PatioFIX.WatchDog" -StartupType Automatic
Remove-Service -Name "PatioFIXWatchDog" ή
sc.exe delete "PatioFIXWatchDog" for PSVersion 5.1




Linked-List
------------------------------------------------------------------
		[<] - NEW DMA ORDER,  ClOrdID=4246                  , LIMIT SELL   500 of 'CH0198251305' @18.6650, by BARCLAYS2, OnBehalfOf = 40124579

		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4251, OrigClOrdID=4246, by BARCLAYS2, OnBehalfOf = 40124579

		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4256, OrigClOrdID=4251, by BARCLAYS2, OnBehalfOf = 40124579


		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4257, OrigClOrdID=4256, by BARCLAYS2, OnBehalfOf = 40124579


		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4279, OrigClOrdID=4257, by BARCLAYS2, OnBehalfOf = 40124579



		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4288, OrigClOrdID=4279, by BARCLAYS2, OnBehalfOf = 40124579



		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4300, OrigClOrdID=4288, by BARCLAYS2, OnBehalfOf = 40124579

		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4306, OrigClOrdID=4300, by BARCLAYS2, OnBehalfOf = 40124579


		[<] - CANCEL/REPLACE REQUEST, ClOrdID=4313, OrigClOrdID=4306, by BARCLAYS2, OnBehalfOf = 40124579


		[<] - CANCEL REQUEST, ClOrdID=4334, OrigClOrdID=4313, by BARCLAYS2, OnBehalfOf = 40124579



