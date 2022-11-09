using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public Node? Search(int key)
        {
            Node? x = root;
            while (x != null && x.Key != key)
            {
                if (key < x.Key)
                    x = x.Left;
                else
                    x = x.Right;
            }
            return x;
        }
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
        public void Delete(int key)
        {
            Node? node = Search(key);
            if (node == null) return;
            Node? x, y = node;
            NodeColor yOriginalColor = y.Color;
            if (node.Left == null)
            {
                x = node.Right;
                Transplant(node, node.Right);
            }
            else if (node.Right == null)
            {
                x = node.Left;
                Transplant(node, node.Left);
            }
            else
            {
                y = Minimum(node.Right);
                yOriginalColor = y.Color;
                x = y.Right;
                if (x != null && y.P == node)
                    x.P = y;
                else
                {
                    Transplant(y, y.Right);
                    y.Right = node.Right;   //insert successor instead of node
                    if (y.Right!= null)
                        y.Right.P = y;//null
                }
                Transplant(node, y);
                y.Left = node.Left;
                y.Left.P = y;
                y.Color = node.Color;
            }
            if (yOriginalColor == NodeColor.BLACK)
                Delete_Fixup(x);
        }
        private void Transplant(Node? u, Node? v)
        {
            if(u.P == null)
                root = v;
            else if (u == u.P.Left)
                u.P.Left = v;
            else
                u.P.Right = v;
            if (v != null)
                v.P = u.P;
        }
        private void Delete_Fixup(Node x)
        {
            Node w; //sibling
            while (x!=null && x != root && x.Color == NodeColor.BLACK)//
            {
                if (x == x.P.Left)  //if left child
                {
                    w = x.P.Right;
                    if (w.Color == NodeColor.RED)
                    {
                        w.Color = NodeColor.BLACK;
                        x.P.Color = NodeColor.RED;
                        LeftRotate(x.P);
                        w = x.P.Right;
                    }
                    if (w.Left.Color == NodeColor.BLACK && w.Right.Color == NodeColor.BLACK)
                    {
                        w.Color = NodeColor.RED;
                        x = x.P;
                    }
                    else
                    {
                        if (w.Right.Color == NodeColor.BLACK)
                        {
                            w.Left.Color = NodeColor.BLACK;
                            w.Color = NodeColor.RED;
                            RightRotate(w);
                            w = x.P.Right;
                        }
                        w.Color = x.P.Color;
                        x.P.Color = NodeColor.BLACK;
                        w.Right.Color = NodeColor.BLACK;
                        LeftRotate(x.P);
                        x = root;
                    }
                }
                else //if right child
                {
                    w = x.P.Left;
                    if (w.Color == NodeColor.RED)
                    {
                        w.Color = NodeColor.BLACK;
                        x.P.Color = NodeColor.RED;
                        RightRotate(x.P);
                        w = x.P.Left;
                    }
                    if (w.Right.Color == NodeColor.BLACK && w.Left.Color == NodeColor.BLACK)
                    {
                        w.Color = NodeColor.RED;
                        x = x.P;
                    }
                    else
                    {
                        if (w.Left.Color == NodeColor.BLACK)
                        {
                            w.Right.Color = NodeColor.BLACK;
                            w.Color = NodeColor.RED;
                            LeftRotate(w);
                            w = x.P.Left;
                        }
                        w.Color = x.P.Color;
                        x.P.Color = NodeColor.BLACK;
                        w.Left.Color = NodeColor.BLACK;
                        RightRotate(x.P);
                        x = root;
                    }
                }
            }
        }
        private Node Minimum(Node node)
        {
            while (node.Left != null)
            {
                node = node.Left;
            }
            return node;
        }
    }
}
