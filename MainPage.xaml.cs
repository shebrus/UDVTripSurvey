using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace UDVTripSurvey;

public partial class MainPage : ContentPage
{
    private bool _isReady;
    private bool _isClearing;

    public MainPage()
    {
        // Отдельные цвета для элементов нужны, чтобы Windows с тёмной темой
        // не отображала светлый текст на светлом фоне.
        if (Application.Current is not null)
            Application.Current.UserAppTheme = AppTheme.Light;

        InitializeComponent();
        _isReady = true;
        RefreshFormState();
    }

    private bool RequiredFieldsValid()
    {
        var organizer = _OrganizerEntry_.Text?.Trim();
        return !string.IsNullOrWhiteSpace(organizer)
               && int.TryParse(_PeopleEntry_.Text?.Trim(), out var count)
               && count >= 1 && count <= 30;
    }

    private void AnswerUpdated()
    {
        if (!_isReady || _isClearing)
            return;

        RefreshFormState();
        _SummaryCard_.IsVisible = false;
    }

    private void RefreshFormState()
    {
        bool complete = RequiredFieldsValid();
        _ShowButton_.IsEnabled = complete;
        _StatusLabel_.IsVisible = complete;
    }

    private void OnRequiredChanged(object? sender, TextChangedEventArgs e) => AnswerUpdated();
    private void OnSelectionChanged(object? sender, CheckedChangedEventArgs e) => AnswerUpdated();
    private void OnTransferChanged(object? sender, ToggledEventArgs e) => AnswerUpdated();
    private void OnDepartureChanged(object? sender, DateChangedEventArgs e) => AnswerUpdated();
    private void OnDurationChanged(object? sender, ValueChangedEventArgs e) => AnswerUpdated();
    private void OnCommentChanged(object? sender, TextChangedEventArgs e) => AnswerUpdated();

    private void OnFormatChanged(object? sender, EventArgs e)
    {
        if (!_isReady || _isClearing)
            return;

        RefreshPickerTitle();
        AnswerUpdated();
    }

    private void RefreshPickerTitle()
    {
        // В Windows свернутый Picker иногда не показывает выбранное значение.
        // Продублируем его в заголовке самого выпадающего элемента.
        var chosen = _FormatPicker_.SelectedIndex >= 0;
        _FormatPicker_.Title = chosen
            ? _FormatPicker_.Items[_FormatPicker_.SelectedIndex]
            : "Выбери формат поездки";
        _FormatPicker_.TitleColor = Color.FromArgb(chosen ? "#21372D" : "#8A9A90");
    }

    private string BudgetText()
    {
        if (_BudgetLow_.IsChecked) return "До 15 000 ₽";
        if (_BudgetMiddle_.IsChecked) return "15–30 тысяч ₽";
        if (_BudgetHigh_.IsChecked) return "30–50 тысяч ₽";
        if (_BudgetFlexible_.IsChecked) return "Больше 50 000 ₽";
        return "Не выбран";
    }

    private void OnShowPlanClicked(object? sender, EventArgs e)
    {
        // Повторная проверка при нажатии кнопки, а не только при вводе.
        if (!RequiredFieldsValid())
        {
            RefreshFormState();
            return;
        }

        var format = _FormatPicker_.SelectedIndex < 0
            ? "Не выбран"
            : _FormatPicker_.Items[_FormatPicker_.SelectedIndex];

        var activities = new List<string>();
        if (_ExcursionsCheck_.IsChecked) activities.Add("Экскурсии и музеи");
        if (_NatureCheck_.IsChecked) activities.Add("Прогулки на природе");
        if (_FoodCheck_.IsChecked) activities.Add("Местная кухня");
        if (_QuietCheck_.IsChecked) activities.Add("Спокойный отдых");

        _SummaryContacts_.Text = $"Организатор: {_OrganizerEntry_.Text?.Trim()}\n" +
                                 $"Участников: {_PeopleEntry_.Text?.Trim()}";
        _SummaryBudget_.Text = $"Формат: {format}\n" +
                               $"Бюджет на человека: {BudgetText()}";
        _SummaryActivities_.Text = $"Пожелания: {(activities.Count > 0 ? string.Join(", ", activities) : "Не выбраны")}\n" +
                                   $"Трансфер: {(_TransferSwitch_.IsToggled ? "Нужен" : "Не нужен")}";
        _SummarySchedule_.Text = $"Дата выезда: {_DepartureDate_.Date:dd.MM.yyyy}\n" +
                                 $"Продолжительность: {(int)_DurationStepper_.Value} дн.";
        _SummaryComment_.Text = "Комментарий: " +
                                (string.IsNullOrWhiteSpace(_CommentEditor_.Text)
                                    ? "Не указан"
                                    : _CommentEditor_.Text.Trim());

        // Не прокручиваем страницу программно: на Windows это иногда
        // неожиданно перебрасывает пользователя к началу формы.
        _SummaryCard_.IsVisible = true;
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        _isClearing = true;
        _OrganizerEntry_.Text = string.Empty;
        _PeopleEntry_.Text = string.Empty;
        _FormatPicker_.SelectedIndex = -1;
        _BudgetLow_.IsChecked = true;
        _BudgetMiddle_.IsChecked = false;
        _BudgetHigh_.IsChecked = false;
        _BudgetFlexible_.IsChecked = false;
        _ExcursionsCheck_.IsChecked = false;
        _NatureCheck_.IsChecked = false;
        _FoodCheck_.IsChecked = false;
        _QuietCheck_.IsChecked = false;
        _TransferSwitch_.IsToggled = false;
        _DepartureDate_.Date = new DateTime(2026, 11, 14);
        _DurationStepper_.Value = 4;
        _CommentEditor_.Text = string.Empty;
        _SummaryContacts_.Text = string.Empty;
        _SummaryBudget_.Text = string.Empty;
        _SummaryActivities_.Text = string.Empty;
        _SummarySchedule_.Text = string.Empty;
        _SummaryComment_.Text = string.Empty;
        _SummaryCard_.IsVisible = false;
        _isClearing = false;

        RefreshPickerTitle();
        RefreshFormState();
    }
}
