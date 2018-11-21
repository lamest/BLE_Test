using System;
using Android.Bluetooth;
using Android.Content;

public class BluetoothStatusBroadcastReceiver : BroadcastReceiver
{
    private readonly Action<State> _stateChangedHandler;

    public BluetoothStatusBroadcastReceiver(Action<State> stateChangedHandler)
    {
        _stateChangedHandler = stateChangedHandler;
    }

    public override void OnReceive(Context context, Intent intent)
    {
        var action = intent.Action;

        if (action != BluetoothAdapter.ActionStateChanged)
            return;

        var state = intent.GetIntExtra(BluetoothAdapter.ExtraState, -1);

        if (state == -1)
        {
            _stateChangedHandler?.Invoke(State.Off);
            return;
        }

        var btState = (State) state;
        _stateChangedHandler?.Invoke(btState);
    }
}