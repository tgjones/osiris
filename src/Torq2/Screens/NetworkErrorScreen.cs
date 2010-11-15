#region File Description
//-----------------------------------------------------------------------------
// NetworkErrorScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Osiris.ScreenManager.Screens;
#endregion

namespace Torq2.Screens
{
    /// <summary>
    /// Specialized message box subclass, used to display network error messages.
    /// </summary>
    class NetworkErrorScreen : MessageBoxScreen
    {
        #region Initialization


        /// <summary>
        /// Constructs an error message box from the specified exception.
        /// </summary>
        public NetworkErrorScreen(Exception exception)
            : base(GetErrorMessage(exception), false)
        { }


        /// <summary>
        /// Converts a network exception into a user friendly error message.
        /// </summary>
        static string GetErrorMessage(Exception exception)
        {
            Trace.WriteLine(string.Format("Network operation threw {0}: {1}",
                                          exception, exception.Message));

            // Is this a GamerPrivilegeException?
            if (exception is GamerPrivilegeException)
            {
                return "You must sign in a suitable gamer profile" + Environment.NewLine + "in order to access this functionality";
            }

            // Is it a NetworkSessionJoinException?
            NetworkSessionJoinException joinException = exception as
                                                            NetworkSessionJoinException;

            if (joinException != null)
            {
                switch (joinException.JoinError)
                {
                    case NetworkSessionJoinError.SessionFull:
											return "This session is already full";

                    case NetworkSessionJoinError.SessionNotFound:
                        return "Session not found. It may have ended," + Environment.NewLine + "or there may be no network connectivity" + Environment.NewLine + "between the local machine and session host";

                    case NetworkSessionJoinError.SessionNotJoinable:
                        return "You must wait for the host to return to" + Environment.NewLine + "the lobby before you can join this session";
                }
            }

            // Otherwise just a generic error message.
            return "There was an error while" + Environment.NewLine + "accessing the network";
        }


        #endregion
    }
}
