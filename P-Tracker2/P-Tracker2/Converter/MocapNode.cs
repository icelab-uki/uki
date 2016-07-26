using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class MocapNode
    {
        
        public double offsetX,offsetY,offsetZ, x , y , z;
        int childIndex;
        double xrotation , yrotation , zrotation;//in Degree
        double tx, ty, tz;
        private String rotationOrder = "zyx";
        //public int numberChannel = 3;
        //
        MocapNode parent = null;

        public String name;
        double[,] matrixM  , rotationMatrix;
        public Boolean isEndsite = false;
        //double[,] positionXYZ = new double[3,1];//[3]

        public void setting(Boolean endsite ,String name , MocapNode parent, double offsetX , double  offsetY, double  offsetZ)
        {
            this.isEndsite = endsite;
            this.name = name;
            this.parent = parent;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.offsetZ = offsetZ;
            if (parent == null) { 
                this.x = offsetX; 
                this.y = offsetY; 
                this.z = offsetZ; 
            }
            else
            {
                this.x = parent.x + offsetX;
                this.y = parent.y + offsetY;
                this.z = parent.z + offsetZ;
            }
        }

        public void setTranslate(string x1, string y1, string z1)
        {
            tx = TheTool.getDouble(x1); 
            ty = TheTool.getDouble(y1);
            tz = TheTool.getDouble(z1);
        }
    
        public void setRotation(string x1, string y1, string z1)
        {
            setRotation(TheTool.getDouble(x1), TheTool.getDouble(y1), TheTool.getDouble(z1));
        }

        public void setRotation(double x1, double y1, double z1)
        {
            xrotation = x1; yrotation = y1; zrotation = z1;
        }

        public void addChild(MocapNode m)
        {
            childIndex++;
        }

        public void createRotationMatrix()
        {
            rotationMatrix = TheBVH.createRotationMatrix(xrotation,yrotation,zrotation,rotationOrder);
        }

        public void tranformRotate()
        {
        
            if(parent == null) // Root
            {
                double[,] matrixT = {{1,0,0,tx},{0,1,0,ty},{0,0,1,tz},{0,0,0,1}};
                matrixM = TheTool.Matrix_Multiply(matrixT,rotationMatrix);
            }
            else
            {
                double[,] matrixT = {{1,0,0,offsetX},{0,1,0,offsetY},{0,0,1,offsetZ},{0,0,0,1}};
                matrixM = TheTool.Matrix_Multiply(parent.matrixM,TheTool.Matrix_Multiply(matrixT,rotationMatrix));
            }
            x = matrixM[0,3];
            y = matrixM[1,3];
            z = matrixM[2,3];
        }

        //public void setPositionXYZ(int noOfFrame) // Create array to store data
        //{
        //    positionXYZ = new double[3,noOfFrame];
        //}

        //public void setXYZ(int frame) // set data xyz to position array
        //{
        //    positionXYZ[0,frame-1] = x;
        //    positionXYZ[1,frame-1] = y;
        //    positionXYZ[2,frame-1] = z;
        //}

        // public void setNumberChannel(int numberChannel) {
        //    this.numberChannel = numberChannel;
        //}

        //public int getNumberChannel() {
        //    return numberChannel;
        //}

        public String toString()
        {
            String msg =  "";
            if(parent==null)
                msg = "===== ROOT "+ name +"======\n";
            else
            {
                msg = "===== Joint "+ name +"======\n";
                msg += "Parent Node : " + parent.name + "\n";
            }
            for(int i= 0 ; i < childIndex ; i++)
            {
                if(i ==0)
                    msg += "Child Nod : " ;
                if(i==childIndex-1)
                    msg += "\n";
            }
            msg += "offset(x , y , z) : "+offsetX+" "+offsetY+" "+offsetZ+"\n";
            msg += "position(x , y , z) : "+x+" "+y+" "+z+"\n";
            msg += "Roation Order : " + rotationOrder + "\n";
            msg += "***************************\n";
            return msg;
        }

    }
}
