using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductAllocationTool
{
    class cornernode:node
    {
        public cornernode(warehouse p_warehouse, int position)
        {
            //Calculate the margin for edge cross aisle
            double margin = p_warehouse.crossaislewidth / 2;
            if (position == 1)
            {
                this.setX(0 + margin);
                this.setY(0 + margin);
            }
            if (position == 2)
            {
                this.setX(p_warehouse.width - margin);
                this.setY(0 + margin);
            }
            if (position == 3)
            {
                this.setX(p_warehouse.width - margin);
                this.setY(p_warehouse.depth - margin);
            }
            if (position == 4)
            {
                this.setX(0 + margin);
                this.setY(p_warehouse.depth - margin);
            }
        }
    }
}
