using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    // Thanh phân trang « ‹ 1 2 3 4 5 › » dùng chung cho mọi danh sách.
    // Cách dùng: dg.ItemsSource = pager.Slice(danhSach);   (gọi pager.Reset() khi đổi bộ lọc)
    public partial class PagerControl : UserControl
    {
        public int PageSize { get; set; } = 8;

        public int CurrentPage { get; private set; } = 1;

        // Báo cho màn hình tải lại danh sách khi người dùng bấm sang trang khác
        public event EventHandler? PageChanged;

        public PagerControl()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void Reset() => CurrentPage = 1;

        // Cắt danh sách theo trang hiện tại và vẽ lại thanh phân trang
        public List<T> Slice<T>(List<T> all)
        {
            int total = all.Count;
            int pages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (CurrentPage > pages) CurrentPage = pages;
            if (CurrentPage < 1) CurrentPage = 1;

            Render(total, pages);
            return all.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        }

        private void Render(int total, int pages)
        {
            panelButtons.Children.Clear();

            // Ít hơn một trang thì ẩn luôn thanh phân trang
            if (total <= PageSize)
            {
                Visibility = Visibility.Collapsed;
                return;
            }
            Visibility = Visibility.Visible;

            panelButtons.Children.Add(MakeButton("«", 1, CurrentPage > 1));
            panelButtons.Children.Add(MakeButton("‹", CurrentPage - 1, CurrentPage > 1));

            // Chỉ hiện tối đa 5 số trang, trượt theo trang hiện tại
            int start = Math.Max(1, Math.Min(CurrentPage - 2, pages - 4));
            int end = Math.Min(pages, start + 4);
            for (int i = start; i <= end; i++)
            {
                panelButtons.Children.Add(MakeButton(i.ToString(), i, true, i == CurrentPage));
            }

            panelButtons.Children.Add(MakeButton("›", CurrentPage + 1, CurrentPage < pages));
            panelButtons.Children.Add(MakeButton("»", pages, CurrentPage < pages));

            int from = (CurrentPage - 1) * PageSize + 1;
            int to = Math.Min(CurrentPage * PageSize, total);
            txtInfo.Text = $"Hiển thị {from}-{to} trên {total}";
        }

        private Button MakeButton(string text, int goTo, bool enabled, bool active = false)
        {
            var btn = new Button
            {
                Content = text,
                Style = (Style)FindResource("PageButton"),
                IsEnabled = enabled,
                Tag = active ? "on" : null
            };
            btn.Click += (s, e) =>
            {
                CurrentPage = goTo;
                PageChanged?.Invoke(this, EventArgs.Empty);
            };
            return btn;
        }
    }
}
