using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductAllocationTool
{
    class exteriornode:node
    {
        private double location;
        private crossaisle top;
        private crossaisle right;
        private crossaisle bottom;
        private crossaisle left;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="top">Top Edge Cross Aisle</param>
        /// <param name="right">Right Edge Cross Aisle</param>
        /// <param name="bottom">Bottom Edge Cross Aisle</param>
        /// <param name="left">Left Edge Cross Aisle</param>
        public exteriornode(crossaisle p_top, crossaisle p_right, crossaisle p_bottom, crossaisle p_left)
        {
            location = 0;
            top = p_top;
            right = p_right;
            bottom = p_bottom;
            left = p_left;
            this.setX(top.getStart().getX());
            this.setY(top.getStart().getY());
        }

        public void setLocation(double p_location)
        {
            location = p_location;
            double tempX;
            double tempY;
            if (location >= 0 && location <= 1)
            {
                if (location >= 0 && location < 0.25)
                {
                    this.setY(top.getStart().getY());
                    tempX = Math.Abs(top.getEnd().getX() - top.getStart().getX()) * ((location - 0) / 0.25) + top.getStart().getX();
                    this.setX(tempX);
                }
                if (location >= 0.25 && location < 0.5)
                {
                    this.setX(right.getStart().getX());
                    tempY = Math.Abs(right.getEnd().getY() - right.getStart().getY()) * ((location - 0.25) / 0.25) + right.getStart().getY();
                    this.setY(tempY);
                }
                if (location >= 0.5 && location < 0.75)
                {
                    this.setY(bottom.getStart().getY());
                    tempX = bottom.getStart().getX() - Math.Abs(bottom.getEnd().getX() - bottom.getStart().getX()) * ((location - 0.5) / 0.25);
                    this.setX(tempX);
                }
                if (location >= 0.75 && location <= 1)
                {
                    this.setX(left.getStart().getX());
                    tempY = left.getStart().getY() - Math.Abs(left.getEnd().getY() - left.getStart().getY()) * ((location - 0.75) / 0.25);
                    this.setY(tempY);
                }
            }
        }

        public double getLocation()
        {
            return location;
        }

        public override double getX()
        {
            setLocation(location);
            return x;
        }

        public override double getY()
        {
            setLocation(location);
            return y;
        }
    }
}
