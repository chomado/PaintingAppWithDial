using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace PaintingAppWithDial
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // 画面に来た時に呼ばれる
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ホイール(Dial)のインスタンス
            var controller = RadialController.CreateForCurrentView();

            // ホイールに追加するメニューを作成（この場合、undo/redo）
            var undoRedoMenuItem = RadialControllerMenuItem
                .CreateFromKnownIcon(displayText: "取り消し/やり直し", value: RadialControllerMenuKnownIcon.UndoRedo);

            // さっき作ったメニューを追加する
            controller.Menu.Items.Add(undoRedoMenuItem);

            // 消した線を取っておく場所
            var undoBuffer = new Stack<InkStroke>();

            // ホイールを回したときに呼ばれる
            controller.RotationChanged += (_, args) =>
            {
                // 今選ばれているメニューは undo/redo である
                if (controller.Menu.GetSelectedMenuItem() == undoRedoMenuItem)
                {
                    // undo
                    if (args.RotationDeltaInDegrees < 0)
                    {
                        // 最後に引かれた線を取ってくる
                        var stroke = inkCanvas.InkPresenter.StrokeContainer.GetStrokes()
                                        .LastOrDefault();

                        // 「最後に引かれた線」があったら
                        if (stroke != null)
                        {
                            // 線を選択状態にする
                            stroke.Selected = true;

                            // 選択した線が消える
                            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();

                            // 消した線を取っておく
                            undoBuffer.Push(stroke);
                        }
                    }
                    // redo
                    else
                    {
                        // 「消した線」が存在したら
                        if (undoBuffer.TryPop(out var stroke))
                        {
                            // 最後に消した線を、画面に戻す
                            inkCanvas.InkPresenter.StrokeContainer
                                    .AddStroke(stroke.Clone());
                        }
                    }
                }
            };

            // 新たに線画引かれた時に呼ばれる
            inkCanvas.InkPresenter.StrokesCollected += (_, args) =>
            {
                // 消した線の情報を破棄する
                undoBuffer.Clear();
            };
        }
    }
}
