using System;
using System.Collections.Generic;
using System.Linq;

namespace ClamAV_Engine.ClamLib
{
    /// <summary>
    /// Aho-Corasick multi-pattern matcher - OPTIMIZED
    /// Quét file 1 lần duy nhất để tìm hàng nghìn mẫu cùng lúc
    /// 
    /// Optimizations:
    /// - Array-based PatternIds thay vì List
    /// - Removed Debug.WriteLine từ scan loop
    /// - Cache-friendly Trie node structure
    /// - Early termination
    /// </summary>
    public class AhoCorasickMatcher
    {
        private class TrieNode
        {
            public TrieNode[] Next = new TrieNode[256]; // 0-255 bytes
            public TrieNode Fail;
            public int[] PatternIds = new int[0]; // Array for fast iteration
            public List<int> PatternIdsList = new List<int>(); // Build-time collection
            
            public void SetPatternIds(int[] ids)
            {
                PatternIds = ids ?? new int[0];
            }
        }

        private TrieNode _root;
        private List<byte[]> _patterns;
        private Dictionary<int, string> _patternNames;

        public AhoCorasickMatcher()
        {
            _root = new TrieNode();
            _patterns = new List<byte[]>();
            _patternNames = new Dictionary<int, string>();
        }

        /// <summary>
        /// Thêm pattern vào Trie
        /// </summary>
        public void AddPattern(byte[] pattern, string name)
        {
            int patternId = _patterns.Count;
            _patterns.Add(pattern);
            _patternNames[patternId] = name;

            TrieNode node = _root;
            foreach (byte b in pattern)
            {
                if (node.Next[b] == null)
                    node.Next[b] = new TrieNode();
                node = node.Next[b];
            }
            // Gắn patternId vào node kết thúc
            node.PatternIdsList.Add(patternId);
        }

        /// <summary>
        /// Build failure links (sau khi add xong tất cả patterns)
        /// </summary>
        public void Build()
        {
            var queue = new Queue<TrieNode>();
            // Chuyển tất cả danh sách PatternIdsList sang mảng để tối ưu truy cập
            var allNodes = new List<TrieNode>();
            CollectNodes(_root, allNodes);
            foreach (var n in allNodes)
            {
                if (n.PatternIdsList != null && n.PatternIdsList.Count > 0)
                    n.SetPatternIds(n.PatternIdsList.ToArray());
            }

            // Level 1: root's children
            for (int i = 0; i < 256; i++)
            {
                if (_root.Next[i] != null)
                {
                    _root.Next[i].Fail = _root;
                    queue.Enqueue(_root.Next[i]);
                }
                else
                {
                    _root.Next[i] = _root; // Shortcut: không có nhánh → quay lại root
                }
            }

            // BFS build failure links
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                for (int i = 0; i < 256; i++)
                {
                    if (node.Next[i] != null)
                    {
                        var fail = node.Fail;
                        while (fail != null && fail.Next[i] == null)
                            fail = fail.Fail;

                        node.Next[i].Fail = (fail ?? _root).Next[i] ?? _root;
                        
                        // Merge fail node pattern ids with current node
                        if ((node.Next[i].Fail?.PatternIds?.Length ?? 0) > 0)
                        {
                            var merged = new List<int>(node.Next[i].PatternIds);
                            merged.AddRange(node.Next[i].Fail.PatternIds);
                            node.Next[i].SetPatternIds(merged.ToArray());
                        }

                        queue.Enqueue(node.Next[i]);
                    }
                }
            }
        }

        private void CollectNodes(TrieNode node, List<TrieNode> list)
        {
            if (node == null) return;
            list.Add(node);
            
            for (int i = 0; i < 256; i++)
            {
                if (node.Next[i] != null && !list.Contains(node.Next[i]))
                    CollectNodes(node.Next[i], list);
            }
        }

        /// <summary>
        /// Quét file 1 lần → trả về list patterns match
        /// OPTIMIZED: Removed debug logging từ hot loop
        /// </summary>
        public List<MatchResult> Scan(byte[] data)
        {
            if (data == null || data.Length == 0)
                return new List<MatchResult>();

            var results = new List<MatchResult>();
            var matchedPatterns = new HashSet<int>(); // Track matched to avoid duplicates

            TrieNode node = _root;
            int position = 0;

            // HOT LOOP: Process each byte with minimal overhead
            foreach (byte b in data)
            {
                while (node != _root && node.Next[b] == null)
                    node = node.Fail;

                node = node.Next[b] ?? _root;

                // Fast iteration over array instead of List
                int[] patternIds = node.PatternIds;
                for (int i = 0; i < patternIds.Length; i++)
                {
                    int patternId = patternIds[i];
                    if (!matchedPatterns.Contains(patternId))
                    {
                        matchedPatterns.Add(patternId);
                        results.Add(new MatchResult
                        {
                            PatternId = patternId,
                            PatternName = _patternNames[patternId],
                            Position = position - _patterns[patternId].Length + 1
                        });
                    }
                }

                position++;
            }

            return results;
        }

        public class MatchResult
        {
            public int PatternId { get; set; }
            public string PatternName { get; set; }
            public long Position { get; set; }
        }
    }
}