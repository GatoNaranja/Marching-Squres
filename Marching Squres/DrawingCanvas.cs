using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region C
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DRV = System.Windows.Media.DrawingVisual;
#endregion

namespace Marching_Squres
{
    public class DrawingCanvas : Canvas
    {
#if !L_DRV_IS_STACKED
        public List<Visual> visuals = new List<Visual>();
#else
        Stack<Visual> UndoStack = new Stack<Visual>();
        Stack<Visual> RedoStack = new Stack<Visual>();
#endif

        //获取Visual的个数
        protected override int VisualChildrenCount
        {
#if !L_DRV_IS_STACKED
            get => visuals.Count;
#else
            get { return UndoStack.Count; }
#endif
        }

        //获取Visual
        protected override Visual GetVisualChild(int index)
        {
#if !L_DRV_IS_STACKED
            return visuals[index];
#else
            List<Visual> arr = UndoStack.ToList();
            arr.Reverse();
            return arr[index];
#endif
        }

        //添加Visual
        public void AddVisual(Visual visual)
        {
#if !L_DRV_IS_STACKED
            visuals.Add(visual);
#else
            UndoStack.Push(visual);
#endif

            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

#if !L_DRV_IS_STACKED
        //删除Visual
        public void RemoveVisual(Visual visual)
        {
            visuals.Remove(visual);

            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);
        }
#else

        public void Undo()
        {
            if (UndoStack.Count == 0) return;
            Visual visual = UndoStack.Pop();

            RedoStack.Push(visual);
            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);
        }

        public void Redo()
        {
            if (RedoStack.Count == 0) return;
            Visual visual = RedoStack.Pop();

            UndoStack.Push(visual);
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }
#endif

        public void Clear()
        {
#if !L_DRV_IS_STACKED
            while (visuals.Count != 0)
                RemoveVisual(visuals[0]);
#else
            while (UndoStack.Count != 0)
                Undo();
#endif
            Children.Clear();
        }

        //命中测试
        public DRV GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DRV;
        }

        private List<DRV> hits = new List<DRV>();

        public List<DRV> GetVisuals(Geometry region)
        {
            hits.Clear();
            GeometryHitTestParameters parameters = new GeometryHitTestParameters(region);
            HitTestResultCallback callback = HitTestCallback;
            VisualTreeHelper.HitTest(this, null, callback, parameters);
            return hits;
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            GeometryHitTestResult geometryResult = (GeometryHitTestResult)result;
            if (result.VisualHit is DRV visual &&
                geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                hits.Add(visual);
            }

            return HitTestResultBehavior.Continue;
        }
    }
}
