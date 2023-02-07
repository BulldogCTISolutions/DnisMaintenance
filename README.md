# DnisMaintenance

Run Locally from within Visual Studio

In "Region" Select, choose 'The Lab'

Select any value within the "DNIS" Select.  The FluentDataGrid should populate.

Set breakpoints in Index.razor at lines 170 and 189

Click either an UP arrow or DOWN arrow.

You should see 'ViewOrder' be different in the '_kvpList'.  I was assuming that '_gridKvpList' would get updated with the call to RefreshDataAsync()
