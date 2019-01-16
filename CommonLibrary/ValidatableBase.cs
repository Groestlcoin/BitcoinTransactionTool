using CommonLibrary.CryptoEncoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NBitcoin;
using NBitcoin.Altcoins;

namespace CommonLibrary {
    public class ValidatableBase : InpcBase, INotifyDataErrorInfo {
        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName) {
            if (string.IsNullOrEmpty(propertyName) || !errors.ContainsKey(propertyName)) {
                return null;
            }
            else {
                return errors[propertyName];
            }
        }

        public bool HasErrors {
            get { return errors.Count > 0; }
        }

        public void AddError(string propertyName, string error) {
            if (!errors.ContainsKey(propertyName)) {
                errors[propertyName] = new List<string>();
            }
            if (!errors[propertyName].Contains(error)) {
                errors[propertyName].Add(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RemoveError(string propertyName, string error) {
            if (errors.ContainsKey(propertyName)) {
                errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RaiseErrorsChanged(string propertyName) {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Validate(string address) {
            if (string.IsNullOrEmpty(address)) {
                AddError(nameof(address), "Address can not be empty!");
                return;
            }
            if (!address.StartsWith("F") && !address.StartsWith("3") && !address.StartsWith("grs1")) {
                AddError(nameof(address), "Address must start with F or 3!");
                return;
            }
            if (address.StartsWith("grs1")) {
                var errors = Bech32.Verify(address, NetworkType.Mainnet);
                if (errors.Any()) {
                    foreach (var error in errors) {
                        AddError(nameof(address), error);
                    }
                    return;
                }
            }
            else {
                try {
                    Groestlcoin.GroestlEncoder.Instance.DecodeData(address);
                }
                catch (Exception e) {
                    AddError(nameof(address), e.Message);
                }
            }
        }
    }
}