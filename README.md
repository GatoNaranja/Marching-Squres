# Marching-Squres
该算法用于为GRAY8图像创建等值线。同样的，在GIS中也能用于创建等高线、等温线、等压线分析等的等值分析。在这里，我们使用的是最基础的Marching Squares算法，并没有加入任何插值校正，就本例来说，对于等值线较为密集的区域，基础算法是远远不够的，如果加入哪怕线性插值，视觉效果上也会大大改观。

二维Marching Squires算法思路，三维通用，较为简单。
首先需要生成二值Map，然后考虑多次贝塞尔插值的问题。
![算法思路](https://user-images.githubusercontent.com/76199161/227443378-5fc38845-cc4c-4c13-a297-c2ab14af537e.jpg)


这里使用的算法经过优化处理保证了执行的效率，但没有使用指针的方式。使用指针也许会更快，但由于当初考虑在Java中使用，因此C#版本仅作演示，这里就不使用指针。为了取得最大的运行速度，在数据量足够大时，可以考虑采用Parallel.For(){}的方式，本例的数据量并不大，为避免线程调度所产生的消耗，仅在注释中保留了并行for循环的方式。

绘图效率上，这里采用了DrawingVisual搭配DrawingCanvas : Canvas的方式减小开销，但是绘图算法上并没有进行严格的优化，相信在绘图上会有更优解。

### 使用
```C#
//此处修改你的图源
BitmapImage bmp = new BitmapImage(new Uri(@"pack://application:,,,/example.bmp"));
```

### 预览效果
![预览](https://user-images.githubusercontent.com/76199161/227457145-087c3320-e799-4b5c-9f8d-0627eda8506f.jpg)
![预览](https://user-images.githubusercontent.com/76199161/227457193-25e0f1b6-9ab4-4e80-84b1-908b4c87420f.jpg)
