using BitcoinTransactionTool.Services;
using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using NBitcoin;
using NBitcoin.Altcoins;
using Transaction = NBitcoin.Transaction;

namespace BitcoinTransactionTool.ViewModels {
    /// <summary>
    /// This part is not yet complete.
    /// It needs more work to add recognizing TxStatus, adding Push feature
    /// and also option to receive input Tx values from Api to calculate Fees
    /// and also adding ability to edit the transaction and create a new Raw Unsigned Tx with the same inputs but different fee.
    /// </summary>
    public class TransactionEditViewModel : ViewModelBase {
        public TransactionEditViewModel() {
            WalletTypeList = new ObservableCollection<WalletType>(Enum.GetValues(typeof(WalletType)).Cast<WalletType>().ToList());
            SelectedWalletType = WalletType.Normal;
            DecodeTxCommand = new RelayCommand(DecodeTx, CanDecodeTx);
        }

        public ObservableCollection<WalletType> WalletTypeList { get; set; }

        private WalletType selectedWalletType;

        public WalletType SelectedWalletType {
            get => selectedWalletType;
            set => SetField(ref selectedWalletType, value);
        }

        private string rawTx;

        public string RawTx {
            get => rawTx;
            set {
                SetField(ref rawTx, value);
                DecodeTxCommand.RaiseCanExecuteChanged();
            }
        }

        private NBitcoin.Transaction trx;

        public NBitcoin.Transaction Trx {
            get => trx;
            set {
                SetField(ref trx, value);
                RaisePropertyChanged(nameof(Inputs));
                RaisePropertyChanged(nameof(Outputs));
            }
        }

        public TxInList Inputs => Trx?.Inputs;
        public List<TxOutExtended> Outputs {
            get {
                List<TxOutExtended> outputs = new List<TxOutExtended>();
                if (Trx?.Outputs != null) {
                    foreach (var output in Trx?.Outputs) {
                        outputs.Add(new TxOutExtended { ScriptPubKey = output.ScriptPubKey, Value = output.Value });
                    }
                }
                return outputs;
            }
        }

        public class TxOutExtended : NBitcoin.TxOut {
            public string PubKey => ScriptPubKey.GetDestinationAddress(Groestlcoin.Instance.Mainnet).ToString();
        }

        public RelayCommand DecodeTxCommand { get; private set; }

        private void DecodeTx() {
            try {
                Trx = Transaction.Parse(RawTx.Trim(), Groestlcoin.Instance.Mainnet);
            }
            catch (Exception ex) {
                MessageBox.Show("Unable to decode transaction, the raw transaction might be invalid.");
            }
        }

       
        private bool CanDecodeTx() {
            return !string.IsNullOrEmpty(RawTx);
        }
    }
}