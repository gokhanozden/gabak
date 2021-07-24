# gabak
GABAK Warehouse Layout Optimizer is a software program developed in C# to optimize, test, compare warehouse layouts for order picking operations.

WHAT IS THE OBJECTIVE FUNCTION?
In short, it is the average travel distance per pick list. In long, we calculate the average travel distance per pick list over a set of pick lists (either generated or real data imported to the software) by solving each pick list tour as a Traveling Salesman Problem and calculating the average travel distance as an objective function.

WHAT OPTIMIZER ARE YOU USING?
We are using a standard evolution strategies (ES) algorithm with single sigma (i.e., a single variance factor for all variables) and a modified 1/5 rule (i.e., if 20 percent of children are more successful than their parents then increase sigma otherwise decrease sigma) to control step size at each iteration. Frequency of sigma change as well as the success ratio (i.e., if you want to use a ratio different than 1/5) are also two parameters that can be changed in the tool. Since the search space is rugged sigma can get very small very quickly if you change sigma very often or success ratio is really high. In our experiments, we believed that success ratio 1/20 performed very well. The other two parameters of ES are Mu (number of parents) and Lambda (number of children). A large number of parents (usually 20 is good) and large number of children (usually 120 to keep 1 to 6 parent to children ratio) will make optimizer to find near optimal results more likely in your runs. However, this will increase the optimization duration as well as computer memory needs.

WHAT CAN YOU DO WITH GABAK (HOW CAN YOU USE IT IN YOUR RESEARCH)?
1. You can find optimal warehouse layouts for order picking operations by using either real pick list data or generated pick list data from the software.
2. You can look at a few different designs and compare their performance based on average travel distance.
3. You can select a single design and import different pick list profiles (either real data or generated pick list profiles from the software). Then you can compare how travel distance is increasing/decreasing with different order profiles.
4. You can compare different path finding methods (aisle centers or visibility graph) and see how much it affects the optimal travel distance calculations. Different path finding methods are creating a distance matrix with different values. Aisle centers assume that pickers are not deviating from center of aisles which have been used so far in warehouse design literature. Visibility graph method assumes that order pickers have a volume and they can walk in a free space by avoiding obstacles (racks, shelfs, pallets, etc.) and move from one pick location to another. Picker size parameter determines the buffer area so order pickers following these new pick paths are not collapsing with obstacles. For example, a forklift truck needs to have a larger clearance than a cart so they don't hit to racks.
5. You can use the example design of experiment excel file to create your own design of experiments to do some analysis. For example, you can create the same warehouse layout but change the pick list size column to observe how pick list size increase is affecting the average travel distance on a warehouse. Also, you can try different number of pick lists sampled in each row to see if there is a significant change in average travel distance. You can also test two or more different layouts with same pick list profiles to compare them.
6. You can also use batch runs excel file to perform batch optimization runs. This is very useful when you are looking for optimal designs for different cases (for example optimal design for uniform demand with pick list size 5 and optimal design for uniform demand with pick list size 10). The software will run optimization for each row one by one and start creating folders under the same folder where excel file is located. The folder structure for each experiment are well organized so you can look at each result easily.

GETTING STARTED
If you would like to only use the tool without building it from the source code, here are the steps:
1.Go to https://www.gokhanozden.com/gabak/publish.htm
2.Click Install.
3.Follow the installation steps.
4.Download and unzip gabak.zip folder under C:/ so it should look like this C:/gabak (this needs to be done manually and you need admin privileges). This part is needed for TSP Solver.

If you would like to build it from source code, you can download any Visual Studio (I am using Visual Studio community edition 2019) and open the solution file once you downloaded the whole project. There are multiple source code files in the project and I suggest you to start with the small files like "SKU.cs" and move to the larger files like "MainForm.cs". The code is fairly well commented out but I will be adding more documents about how each folder is being used.

If you have any questions please send me an e-mail gokhan@psu.edu or gokhan.ozden@yahoo.com.
