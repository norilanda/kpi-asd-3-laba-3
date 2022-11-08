using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo
{
    public enum NodeColor
    {
        RED,
        BLACK
    }
    public class Node
    {
        private NodeColor color;    //node color (black/red)
        private int key;
        private int data;
        private Node? leftChild;
        private Node? rightChild;
        private Node? parent;

        public Node(int key, int data)
        {
            this.key = key;
            this.data = data;
            this.leftChild = null;
            this.rightChild = null;
            this.parent = null;
            this.color = NodeColor.RED;
        }

        /*  getters/setters */
        public Node? Left
        {
            get => leftChild;
            set => leftChild = value;
        }
        public Node? Right
        {
            get => rightChild;
            set => rightChild = value;
        }
        public Node P
        {
            get => parent;
            set => parent = value;
        }
        public Node G
        {
            get => parent.parent;
        }
        public NodeColor Color
        {
            get => color;
            set => color = value;
        }
        public int Key
        {
            get => key;
        }

    }
}
