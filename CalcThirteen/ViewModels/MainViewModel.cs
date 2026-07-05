using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalcThirteen.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private string _displayText = "0";
    private string _expression = "";

    public string DisplayText
    {
        get => _displayText;
        set => SetProperty(ref _displayText, value);
    }

    public string Expression
    {
        get => _expression;
        set => SetProperty(ref _expression, value);
    }

    public ObservableCollection<string> HistoryLog { get; } = new();

    private long? _firstOperand = null;
    private string _currentOperator = "";
    private bool _isNewEntry = true;

    public IRelayCommand<string> DigitCommand { get; }
    public IRelayCommand<string> OperatorCommand { get; }
    public IRelayCommand CalculateCommand { get; }
    public IRelayCommand ClearCommand { get; }
    public IRelayCommand BackspaceCommand { get; }

    public MainViewModel()
    {
        DigitCommand = new RelayCommand<string>(OnDigit);
        OperatorCommand = new RelayCommand<string>(OnOperator);
        CalculateCommand = new RelayCommand(OnCalculate);
        ClearCommand = new RelayCommand(OnClear);
        BackspaceCommand = new RelayCommand(OnBackspace);
    }

    private long ExecuteMath(long first, long second, string op)
    {
        checked
        {
            switch (op)
            {
                case "+": return first + second;
                case "-": return first - second;
                case "*": return first * second;
                case "/":
                    if (second == 0) throw new DivideByZeroException();
                    return first / second;
                default: throw new InvalidOperationException("Неизвестный оператор");
            }
        }
    }

    private void OnDigit(string? digit)
    {
        if (string.IsNullOrEmpty(digit)) return;

        
        if ((_isNewEntry || DisplayText == "0") && DisplayText != "-")
        {
            DisplayText = digit;
            _isNewEntry = false;
        }
        else
        {
           
            DisplayText += digit;
        }
    }

    private void OnOperator(string? op)
    {
        if (string.IsNullOrEmpty(op)) return;

        
        if (op == "-" && (_isNewEntry || DisplayText == "0"))
        {
            DisplayText = "-";
            _isNewEntry = false;
            return;
        }

        try
        {
            if (DisplayText == "-") return;

            
            if (_firstOperand != null && !string.IsNullOrEmpty(_currentOperator) && !_isNewEntry)
            {
                long secondOperand = FromBase13(DisplayText);
                long result = ExecuteMath(_firstOperand.Value, secondOperand, _currentOperator);

                
                string resultStr = ToBase13(result);
                string fullRecord = $"{ToBase13(_firstOperand.Value)} {_currentOperator} {ToBase13(secondOperand)} = {resultStr}";
                HistoryLog.Insert(0, fullRecord);

                
                DisplayText = resultStr;
            }

            
            _firstOperand = FromBase13(DisplayText);
            _currentOperator = op;
            Expression = $"{DisplayText.Trim().ToUpper()} {op}";
            _isNewEntry = true;
        }
        catch (OverflowException)
        {
            DisplayText = "OVERFLOW ERROR";
            OnClear();
        }
        catch (DivideByZeroException)
        {
            DisplayText = "ДЕЛЕНИЕ НА 0";
            OnClear();
        }
        catch
        {
            DisplayText = "ERROR";
            OnClear();
        }
    }

    private void OnCalculate()
    {
        if (_firstOperand == null || string.IsNullOrEmpty(_currentOperator)) return;

        try
        {
            long secondOperand = FromBase13(DisplayText);
            long result = ExecuteMath(_firstOperand.Value, secondOperand, _currentOperator);

            string resultStr = ToBase13(result);
            string fullRecord = $"{ToBase13(_firstOperand.Value)} {_currentOperator} {ToBase13(secondOperand)} = {resultStr}";
            HistoryLog.Insert(0, fullRecord);

            DisplayText = resultStr;
            Expression = "";
            _firstOperand = null;
            _currentOperator = "";
            _isNewEntry = true;
        }
        catch (OverflowException)
        {
            DisplayText = "OVERFLOW ERROR";
            Expression = "";
            _firstOperand = null;
            _currentOperator = "";
            _isNewEntry = true;
        }
        catch (DivideByZeroException)
        {
            DisplayText = "ДЕЛЕНИЕ НА 0";
            Expression = "";
            _firstOperand = null;
            _currentOperator = "";
            _isNewEntry = true;
        }
        catch
        {
            DisplayText = "ERROR";
        }
    }

    private void OnClear()
    {
        DisplayText = "0";
        Expression = "";
        _firstOperand = null;
        _currentOperator = "";
        _isNewEntry = true;
    }

    private void OnBackspace()
    {
        if (_isNewEntry || DisplayText.Length <= 1)
        {
            DisplayText = "0";
            _isNewEntry = true;
        }
        else
        {
            DisplayText = DisplayText.Substring(0, DisplayText.Length - 1);
        }
    }

    private static long FromBase13(string value)
    {
        value = value.Trim().ToUpper();
        if (string.IsNullOrEmpty(value)) return 0;

        
        bool isNegative = false;
        if (value.StartsWith("-"))
        {
            isNegative = true;
            value = value.Substring(1); 
        }

        
        if (string.IsNullOrEmpty(value)) return 0;

        long result = 0;
        string digits = "0123456789ABC";

        foreach (char c in value)
        {
            int digitValue = digits.IndexOf(c);
            if (digitValue == -1)
                throw new FormatException($"Недопустимый символ в Base 13: {c}");

            result = result * 13 + digitValue;
        }

        return isNegative ? -result : result;
    }

    private static string ToBase13(long value)
    {
        if (value == 0) return "0";

        bool isNegative = value < 0;
        if (isNegative) value = -value;

        string digits = "0123456789ABC";
        string result = "";

        while (value > 0)
        {
            int remainder = (int)(value % 13);
            result = digits[remainder] + result;
            value /= 13;
        }

        return isNegative ? "-" + result : result;
    }
}