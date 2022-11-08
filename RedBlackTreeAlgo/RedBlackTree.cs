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
        public bool Insert(int key, int value)
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
            return InsertFixup(node);
        }
        private bool InsertFixup(Node node)
        {
            try
            {
                while (node.P != null && node.P.Color == NodeColor.RED)
                {
                    if (node.P == node.G.Left)   //if parent is a left child
                    {
                        Node y = node.G.Right;  //uncle
                        if (y != null && y.Color == NodeColor.RED)  //case 1 (uncle is RED). Solution: recolor
                        {
                            node.P.Color = NodeColor.BLACK;
                            y.Color = NodeColor.BLACK;
                            node.G.Color = NodeColor.RED;
                            node = node.G;
                        }
                        else 
                        {
                            if (node == node.P.Right)   //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                            {                                
                                node = node.P;
                                LeftRotate(node);
                            }
                            node.P.Color = NodeColor.BLACK;    //case 3 (uncle is black, line). Solution: recolor and rotate
                            node.G.Color = NodeColor.RED;
                            RightRotate(node.G);
                        }
                    }
                    else //if parent is a right child
                    {
                        Node y = node.G.Left;   //uncle
                        if (y != null && y.Color == NodeColor.RED)  //case 1 (uncle is RED). Solution: recolor
                        {
                            node.P.Color = NodeColor.BLACK;
                            y.Color = NodeColor.BLACK;
                            node.G.Color = NodeColor.RED;
                            node = node.G;
                        }
                        else 
                        {
                            if (node == node.P.Left)   //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                            {
                                node = node.P;
                                RightRotate(node);
                            }
                            node.P.Color = NodeColor.BLACK;    //case 3 (uncle is black, line). Solution: recolor and rotate
                            node.G.Color = NodeColor.RED;
                            LeftRotate(node.G);
                        }                        
                    }
                }
                root.Color = NodeColor.BLACK;   //case 0 (node is root)
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        private void LeftRotate(Node x)
        {
            Node y = x.Right;
            x.Right = y.Left;
            if (y.Left != null)
                y.Left.P = x;
            y.P = x.P;
            if (x.P == null)
                root = y;
            else if (x == x.P.Left)
                x.P.Left = y;
            else
                x.P.Right = y;
            y.Left = x;
            x.P = y;
        }
        private void RightRotate(Node x)
        {
            Node y = x.Left;
            x.Left = y.Right;
            if (y.Right != null)
                y.Right.P = x;
            y.P = x.P;
            if (x.P == null)
                root = y;
            else if (x == x.P.Right)
                x.P.Right = y;
            else
                x.P.Left = y;
            y.Right = x;
            x.P = y;
        }

        public Node? Search(int key)
        {
            Node? x = root;
            while(x != null && x.Key != key)
            {
                if (key < x.Key)
                    x = x.Left;
                else
                    x = x.Right;
            }
            return x;
        }
    }
}
