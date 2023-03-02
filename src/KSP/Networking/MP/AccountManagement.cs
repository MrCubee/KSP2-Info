using KSP.Game;
using KSP.Networking.MP.Utils;
using KSP.Networking.OnlineServices.Authentication;
using System;

namespace KSP.Networking.MP {
    public class AccountManagement {
        public
        const AccountManagementState DEFAULT_ACCOUNT_MANAGEMENT_STATE = AccountManagementState.None;
        public
        const AccountManagementOperation DEFAULT_ACCOUNT_MANAGEMENT_OPERATION = AccountManagementOperation.None;
        public
        const AccountLoginCodeState DEFAULT_ACCOUNT_LOGINCODE_STATE = AccountLoginCodeState.None;
        private AccountManagementState _accountManagementState;
        private long _timestampStateChanged;
        private AccountManagementOperation _accountManagementOperation;
        private bool _isBypass;
        private AccountLoginCodeState _accountLoginCodeState;
        private string _errorString = "";
        private string _userLoginCode = "";
        private bool _accountWindowOpened;
        private bool _isAuthorizationComplete;
        private bool _openBrowserOnLogin;
        private bool _isLoginProcessStarted;
        public int LoginRetryDelay = 1000;

        public static GameInstance Game => GameManager.Instance.Game;

        public AccountManagementState AccountManagementState => this._accountManagementState;

        public bool IsLoggedOut => this._accountManagementState == AccountManagementState.LoggedOut || this._accountManagementState == AccountManagementState.None || this._accountManagementState == AccountManagementState.Error;

        public bool IsLoggingIn => this._accountManagementState == AccountManagementState.LoggingIn;

        public bool IsLoggedIn => this._accountManagementState == AccountManagementState.LoggedIn;

        public bool IsLoggingOut => this._accountManagementState == AccountManagementState.LoggingOut;

        public AccountLoginCodeState AccountLoginCodeState => this._accountLoginCodeState;

        public bool AccountCodeNotReceived => this._accountLoginCodeState == AccountLoginCodeState.None || this._accountLoginCodeState == AccountLoginCodeState.Error;

        public bool IsAccountCodeReceiving => this._accountLoginCodeState == AccountLoginCodeState.ReceivingCode;

        public bool IsAccountCodeReceived => this._accountLoginCodeState == AccountLoginCodeState.ReceivedCode;

        public int MSInCurrentState => this._timestampStateChanged != 0 L ? (int)(TimeUtil.GetMS() - this._timestampStateChanged) : -1;

        public string TimeInCurrentStateString => this._timestampStateChanged != 0 L ? TimeUtil.GetDHMSMSString((long) this.MSInCurrentState, false, false) : "N/A";

        public AccountManagementOperation AccountManagementOperation => this._accountManagementOperation;

        public bool IsBypass => this._isBypass;

        public string UserLoginCode => this._userLoginCode;

        public string ErrorString => this._errorString;

        public void SetErrorString(string errorString) => this._errorString = errorString ?? "";

        public void ClearErrorString() => this._errorString = "";

        public AccountManagement() => this.Init();

        ~AccountManagement() => this.Uninit();

        public void Init() {
            this.Uninit();
            this._accountManagementState = AccountManagementState.None;
            this._timestampStateChanged = 0 L;
            this._accountManagementOperation = AccountManagementOperation.None;
            this._isBypass = false;
            this._accountLoginCodeState = AccountLoginCodeState.None;
            this.ClearErrorString();
            this.GotoAccountManagementState(AccountManagementState.LoggedOut);
            this.GotoAccountLoginCodeState(AccountLoginCodeState.None);
        }

        public void Uninit() {
            this.GotoAccountManagementState(AccountManagementState.None);
            this.GotoAccountLoginCodeState(AccountLoginCodeState.None);
            this._accountManagementState = AccountManagementState.None;
            this._timestampStateChanged = 0 L;
            this._accountManagementOperation = AccountManagementOperation.None;
            this._isBypass = false;
            this._isLoginProcessStarted = false;
            this._accountLoginCodeState = AccountLoginCodeState.None;
            this.ClearErrorString();
        }

        private void GotoAccountManagementState(
            AccountManagementState accountManagementState,
            bool force = false) {
            AccountManagementState accountManagementState1 = this._accountManagementState;
            AccountManagementState accountManagementState2 = accountManagementState;
            if (accountManagementState2 == accountManagementState1 && !force)
                return;
            switch (accountManagementState1) {
            default:
                this._accountManagementState = accountManagementState2;
                this._timestampStateChanged = TimeUtil.GetMS();
                switch (accountManagementState2) {
                case AccountManagementState.None:
                    return;
                case AccountManagementState.LoggedOut:
                    this._accountManagementOperation = AccountManagementOperation.None;
                    this._isBypass = false;
                    return;
                case AccountManagementState.LoggingIn:
                    return;
                case AccountManagementState.LoggingOut:
                    return;
                case AccountManagementState.LoggedIn:
                    this._accountManagementOperation = AccountManagementOperation.None;
                    return;
                case AccountManagementState.Error:
                    this._accountManagementOperation = AccountManagementOperation.None;
                    this._isBypass = false;
                    return;
                default:
                    return;
                }
            }
        }

        private void GotoAccountLoginCodeState(AccountLoginCodeState accountLoginCodeState, bool force = false) {
            AccountLoginCodeState accountLoginCodeState1 = this._accountLoginCodeState;
            AccountLoginCodeState accountLoginCodeState2 = accountLoginCodeState;
            if (accountLoginCodeState2 == accountLoginCodeState1 && !force)
                return;
            switch (accountLoginCodeState1) {
            default:
                this._accountLoginCodeState = accountLoginCodeState2;
                this._timestampStateChanged = TimeUtil.GetMS();
                switch (accountLoginCodeState2) {
                case AccountLoginCodeState.None:
                    return;
                case AccountLoginCodeState.ReceivingCode:
                    return;
                case AccountLoginCodeState.ReceivedCode:
                    return;
                case AccountLoginCodeState.Error:
                    return;
                default:
                    return;
                }
            }
        }

        private void PumpLoggedOut() {}

        private void PumpLoggingIn() {
            if (this._isBypass) {
                if (this.MSInCurrentState <= 0)
                    return;
                this.GotoAccountManagementState(AccountManagementState.LoggedIn);
            } else if (this._openBrowserOnLogin) {
                if (!this._accountWindowOpened)
                    this.OpenAccountWindowOnReady();
                else if (this.MSInCurrentState > this.LoginRetryDelay && !this._accountWindowOpened)
                    this.OpenAccountWindowOnReady();
                if (this._isAuthorizationComplete || !this._accountWindowOpened)
                    return;
                this.CheckAuthorizationStatus();
            } else {
                if (string.IsNullOrEmpty(AuthenticationManager.UserCode))
                    return;
                this._userLoginCode = AuthenticationManager.UserCode;
                if (this._isAuthorizationComplete)
                    return;
                this.CheckAuthorizationStatus();
            }
        }

        private void PumpLoggingOut() {
            if (this._isBypass) {
                if (this.MSInCurrentState <= 0)
                    return;
                this.GotoAccountManagementState(AccountManagementState.LoggedOut);
            } else
                this.GotoAccountManagementState(AccountManagementState.LoggedOut);
        }

        private void PumpLoggedIn() {}

        private void PumpError() {}

        public void Pump() {
            switch (this._accountManagementState) {
            case AccountManagementState.LoggedOut:
                this.PumpLoggedOut();
                break;
            case AccountManagementState.LoggingIn:
                this.PumpLoggingIn();
                break;
            case AccountManagementState.LoggingOut:
                this.PumpLoggingOut();
                break;
            case AccountManagementState.LoggedIn:
                this.PumpLoggedIn();
                break;
            case AccountManagementState.Error:
                this.PumpError();
                break;
            }
        }

        private void PumpReceivingUserCode() {
            if (!this._isLoginProcessStarted) {
                this._isLoginProcessStarted = true;
                AuthenticationManager.Instance.InitiateLogin();
            }
            if (!AuthenticationManager.LoginLinkReady)
                return;
            this._userLoginCode = AuthenticationManager.UserCode;
            this.GotoAccountLoginCodeState(AccountLoginCodeState.ReceivedCode);
        }

        private void PumpReceivedUserCode() {
            if (AuthenticationManager.LoginLinkReady)
                return;
            this.GotoAccountLoginCodeState(AccountLoginCodeState.Error);
        }

        private void PumpErrorUserCode() {}

        public void PumpUserCode() {
            switch (this._accountLoginCodeState) {
            case AccountLoginCodeState.ReceivingCode:
                this.PumpReceivingUserCode();
                break;
            case AccountLoginCodeState.ReceivedCode:
                this.PumpReceivedUserCode();
                break;
            case AccountLoginCodeState.Error:
                this.PumpError();
                break;
            }
        }

        private void OpenAccountWindowOnReady() {
            if (string.IsNullOrEmpty(AuthenticationManager.LoginLink))
                return;
            this._accountWindowOpened = true;
            Util.OpenURLInExternalBrowser(AuthenticationManager.LoginLink);
        }

        private void CheckAuthorizationStatus() {
            if (!AuthenticationManager.AuthenticationComplete) {
                if (this.MSInCurrentState <= this.LoginRetryDelay)
                    return;
                AuthenticationManager.Instance.FinalizeLogin(AuthenticationManager.DeviceCode);
            } else if (AuthenticationManager.AuthenticationComplete) {
                this._isAuthorizationComplete = true;
                this.GotoAccountManagementState(AccountManagementState.LoggedIn);
            } else {
                if (this.MSInCurrentState <= this.LoginRetryDelay)
                    return;
                int num = AuthenticationManager.AuthenticationComplete ? 1 : 0;
            }
        }

        public bool StartLoggingIn(
            AccountManagementLogInMode accountManagementLogInMode,
            out string errorStringOut,
            string accountNameString = "",
            string accountPasswordString = "") {
            errorStringOut = "";
            this.ClearErrorString();
            accountNameString = accountNameString ?? "";
            accountPasswordString = accountPasswordString ?? "";
            if (this._accountManagementState != AccountManagementState.LoggedOut && this._accountManagementState != AccountManagementState.Error) {
                errorStringOut = "Unable to log in because current state is " + Convert.ToString((object) this._accountManagementState);
                this.SetErrorString(errorStringOut);
                return false;
            }
            this._accountManagementOperation = AccountManagementOperation.None;
            this.GotoAccountManagementState(AccountManagementState.LoggingIn);
            switch (accountManagementLogInMode) {
            case AccountManagementLogInMode.None:
                errorStringOut = "Unable to log in because LogInMode is " + Convert.ToString((object) accountManagementLogInMode);
                this.SetErrorString(errorStringOut);
                this.GotoAccountManagementState(AccountManagementState.Error);
                return false;
            case AccountManagementLogInMode.OnlineBrowser:
                this._accountManagementOperation = AccountManagementOperation.LoggingInNormal;
                this._isBypass = false;
                this._openBrowserOnLogin = true;
                AuthenticationManager.Instance.InitiateLogin();
                break;
            case AccountManagementLogInMode.DebugBypass:
                this._accountManagementOperation = AccountManagementOperation.LoggingInBypass;
                this._isBypass = true;
                break;
            default:
                errorStringOut = "Unable to log in because LogInMode is " + Convert.ToString((object) accountManagementLogInMode);
                this.SetErrorString(errorStringOut);
                this.GotoAccountManagementState(AccountManagementState.Error);
                return false;
            }
            return true;
        }

        public bool CancelLoggingIn(out string errorStringOut) {
            errorStringOut = "";
            if (this._accountManagementState == AccountManagementState.LoggingIn) {
                this._accountManagementOperation = AccountManagementOperation.LoggingOut;
                this.GotoAccountManagementState(AccountManagementState.LoggingOut);
                if (!this._isBypass) {
                    this._accountWindowOpened = false;
                    this._isAuthorizationComplete = false;
                    this._isLoginProcessStarted = false;
                    AuthenticationManager.Instance.Logout();
                }
                return true;
            }
            errorStringOut = "Unable to log in because LogInMode is " + Convert.ToString((object) this._accountManagementState);
            return false;
        }

        public bool StartLoggingOut(out string errorStringOut) {
            errorStringOut = "";
            this.ClearErrorString();
            if (this._accountManagementState != AccountManagementState.LoggedIn) {
                errorStringOut = "Unable to log out because current state is " + Convert.ToString((object) this._accountManagementState);
                this.SetErrorString(errorStringOut);
                return false;
            }
            this._accountManagementOperation = AccountManagementOperation.LoggingOut;
            this.GotoAccountManagementState(AccountManagementState.LoggingOut);
            if (!this._isBypass) {
                this._accountWindowOpened = false;
                this._isAuthorizationComplete = false;
                this._isLoginProcessStarted = false;
                AuthenticationManager.Instance.Logout();
            }
            return true;
        }

        public bool StartFetchingUserCode(out string errorStringOut) {
            errorStringOut = "";
            this.ClearErrorString();
            if (this._accountLoginCodeState == AccountLoginCodeState.None && this._accountLoginCodeState == AccountLoginCodeState.Error) {
                errorStringOut = "Unable to fetch log in code because current state is " + Convert.ToString((object) this._accountManagementState);
                this.SetErrorString(errorStringOut);
                return false;
            }
            this.GotoAccountLoginCodeState(AccountLoginCodeState.ReceivingCode);
            return true;
        }

        public bool CancelFetchingUserCode(out string errorStringOut) {
            errorStringOut = "";
            this.ClearErrorString();
            if (this._accountLoginCodeState == AccountLoginCodeState.ReceivingCode) {
                this.GotoAccountLoginCodeState(AccountLoginCodeState.None);
                this._isAuthorizationComplete = false;
                this._isLoginProcessStarted = false;
                return true;
            }
            errorStringOut = "Cannot cancel retrieving login code, as it hasn't started";
            this.SetErrorString(errorStringOut);
            return false;
        }

        public string AccountNameString {
            get {
                if (this.AccountManagementState != AccountManagementState.LoggedIn)
                    return "LOGGEDOUT";
                return this.IsBypass ? "DEBUGBYPASS" : AuthenticationManager.OnlinePlayerNameString;
            }
        }

        public string AccountGuidString {
            get {
                if (this.AccountManagementState != AccountManagementState.LoggedIn)
                    return "LOGGEDOUTGUID";
                return this.IsBypass ? "DEBUGBYPASSGUID" : AuthenticationManager.OnlinePlayerGuidString;
            }
        }

        public string ActivePlayerNameString {
            get {
                if (this.AccountManagementState != AccountManagementState.LoggedIn)
                    return AccountManagement.Game.MP.DebugVars.LoggedOutPlayerNameString;
                return this.IsBypass ? AccountManagement.Game.MP.DebugVars.BypassPlayerNameString : AuthenticationManager.OnlinePlayerNameString;
            }
        }

        public string ActivePlayerGuidString {
            get {
                if (this.AccountManagementState != AccountManagementState.LoggedIn)
                    return AccountManagement.Game.MP.DebugVars.LoggedOutPlayerGuidString;
                return this.IsBypass ? AccountManagement.Game.MP.DebugVars.BypassPlayerGuidString : AuthenticationManager.OnlinePlayerGuidString;
            }
        }
    }
}