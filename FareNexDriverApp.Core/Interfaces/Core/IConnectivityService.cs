using System;

namespace FareNexDriverApp.Core.Interfaces.Core; 
public interface IConnectivityService
{
    event EventHandler<bool>? OnConnetionStatusChanged;
    void StartConnectivityListner();
    bool IsNetworkAvailable(); 
}