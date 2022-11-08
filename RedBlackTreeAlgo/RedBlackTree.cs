using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo
{
    public class RedBlackTree
    {
        private Node? root;

        public RedBlackTree()
        {
            root = null;
        }
        public Node? Root => root;
        public void Insert(int key, int value)
        {
            Node? y = null;
            Node? x = root;
            while (x != null)
            {
                y = x;
                if (key < x.Key)
                    x = x.Left;
                else
                    x = x.Right;
            }
            Node node = new Node(key, value);
            node.P = y;
            if (y == null)
                root = node;
            else if (key < y.Key)
                y.Left = node;
            else y.Right = node;
            InsertFixup(node);
        }
        private void InsertFixup(Node node)
        {
            while (node.P != null && node.P.Color == NodeColor.RED)
            {
                if (node.P == node.G.Left)   //if parent is a left child
                {
                    Node y = node.G.Right;
                    if (y != null && y.Color == NodeColor.RED)
                    {
                        node.P.Color = NodeColor.BLACK;
                        y.Color = NodeColor.BLACK;
                        node.G.Color = NodeColor.RED;
                        node = node.G;
                    }
                    else if (node == node.P.Right)
                    {
                        node = node.P;
                        LeftRotate(node);
                        node.P.Color = NodeColor.BLACK;
                        node.G.Color = NodeColor.RED;
                        RightRotate(node.G);
                    }
                }
                else //if parent is a right child
                {
                    Node y = node.G.Left;
                    if (y != null && y.Color == NodeColor.RED)
                    {
                        node.P.Color = NodeColor.BLACK;
                        y.Color = NodeColor.BLACK;
                        node.G.Color = NodeColor.RED;
                        node = node.G;
                    }
                    else if (node == node.P.Left)
                    {
                        node = node.P;
                        RightRotate(node);
                        node.P.Color = NodeColor.BLACK;
                        node.G.Color = NodeColor.RED;
                        LeftRotate(node.G);
                    }
                }
            }
            root.Color = NodeColor.BLACK;
        }
        private void LeftRotate(Node x)
        {
            Node temp = x.Right;
            x.Right = temp.Left;
            temp.Left.P = x;
            temp.P = x.P;
            if (x.P == null)
                root = temp;
            else if (x == x.P.Left)
                x.P.Left = temp;
            else
                x.P.Right = temp;
            temp.Left = x;
            x.P = temp;
        }
        private void RightRotate(Node x)
        {
            Node temp = x.Left;
            x.Left = temp.Right;
            temp.Right.P = x;
            temp.P = x.P;
            if (x.P == null)
                root = temp;
            else if (x == x.P.Right)
                x.P.Right = temp;
            else
                x.P.Left = temp;
            temp.Right = x;
            x.P = temp;
        }
    }
}
