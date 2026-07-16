import React, { useState, useEffect, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import apiClient from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { Send, MessageCircle, User, Store, Package } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { Role } from '../types';

interface LocalMessage {
  id: string;
  senderId: number;
  senderEmail: string;
  recipientId: number;
  recipientEmail: string;
  productId?: number;
  productName?: string;
  messageText: string;
  sentAt: string;
}

interface ChatThread {
  partyId: number;
  partyEmail: string;
  partyName: string;
  isStore: boolean;
  lastMessage: string;
  lastMessageTime: string;
  messages: LocalMessage[];
}

const Messages: React.FC = () => {
  const { userId, email, role } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  const targetProduct = searchParams.get('product');
  const targetStore = searchParams.get('store');

  const [threads, setThreads] = useState<ChatThread[]>([]);
  const [activeThreadIdx, setActiveThreadIdx] = useState<number | null>(null);
  const [inputMessage, setInputMessage] = useState('');
  const [isSending, setIsSending] = useState(false);
  
  const chatContainerRef = useRef<HTMLDivElement>(null);

  // Scroll to bottom of chat locally to prevent window jumping
  const scrollToBottom = () => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop = chatContainerRef.current.scrollHeight;
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [activeThreadIdx, threads]);

  // Load chat history from LocalStorage (provides a robust simulation since backend lacks user-scoped fetch)
  useEffect(() => {
    const savedChats = localStorage.getItem(`elAtaba_chats_${userId}`);
    let loadedThreads: ChatThread[] = savedChats ? JSON.parse(savedChats) : [];

    // If navigated here from a product page, initialize that conversation
    if (targetStore && targetProduct && userId) {
      const storeIdNum = Number(targetStore);
      const prodIdNum = Number(targetProduct);
      
      // Look for existing thread with this store
      let threadIdx = loadedThreads.findIndex(t => t.partyId === storeIdNum && t.isStore);
      
      if (threadIdx === -1) {
        // Create a new thread
        const newThread: ChatThread = {
          partyId: storeIdNum,
          partyEmail: `store${storeIdNum}@elataba.com`,
          partyName: `Store #${storeIdNum} (Seller)`,
          isStore: true,
          lastMessage: "Inquiry about product",
          lastMessageTime: new Date().toISOString(),
          messages: [
            {
              id: 'init',
              senderId: storeIdNum,
              senderEmail: `store${storeIdNum}@elataba.com`,
              recipientId: userId,
              recipientEmail: email || 'buyer@elataba.com',
              productId: prodIdNum,
              productName: `Product #${prodIdNum}`,
              messageText: "Hello! How can we help you with this wholesale item?",
              sentAt: new Date(Date.now() - 3600000).toISOString() // 1 hour ago
            }
          ]
        };
        loadedThreads = [newThread, ...loadedThreads];
        threadIdx = 0;
      }
      setThreads(loadedThreads);
      setActiveThreadIdx(threadIdx);
    } else {
      // Default loaded threads
      if (loadedThreads.length === 0 && userId) {
        // Load default mock thread for demo purposes
        loadedThreads = [
          {
            partyId: 99,
            partyEmail: 'support@elataba.com',
            partyName: 'ElAtaba Market Support',
            isStore: false,
            lastMessage: 'Welcome to the wholesale marketplace!',
            lastMessageTime: new Date().toISOString(),
            messages: [
              {
                id: 'welcome',
                senderId: 99,
                senderEmail: 'support@elataba.com',
                recipientId: userId || 0,
                recipientEmail: email || '',
                messageText: 'Welcome to ElAtaba! Let us know if you have any wholesale inquiries or store registration questions.',
                sentAt: new Date().toISOString()
              }
            ]
          }
        ];
      }
      setThreads(loadedThreads);
      if (loadedThreads.length > 0) {
        setActiveThreadIdx(0);
      }
    }
  }, [targetStore, targetProduct, userId, email]);

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputMessage.trim() || activeThreadIdx === null || !userId) return;

    const currentThread = threads[activeThreadIdx];
    setIsSending(true);

    try {
      const payload = {
        senderId: userId,
        recipientId: currentThread.partyId,
        productId: targetProduct ? Number(targetProduct) : null,
        messageText: inputMessage.trim()
      };

      // Call the actual backend MessageController endpoint
      await apiClient.post('/api/Message', payload);

      // Create local message object
      const newMsg: LocalMessage = {
        id: Math.random().toString(),
        senderId: userId,
        senderEmail: email || 'buyer@elataba.com',
        recipientId: currentThread.partyId,
        recipientEmail: currentThread.partyEmail,
        productId: targetProduct ? Number(targetProduct) : undefined,
        productName: targetProduct ? `Product #${targetProduct}` : undefined,
        messageText: inputMessage.trim(),
        sentAt: new Date().toISOString()
      };

      // Update local state
      const updatedThreads = [...threads];
      updatedThreads[activeThreadIdx] = {
        ...currentThread,
        lastMessage: inputMessage.trim(),
        lastMessageTime: new Date().toISOString(),
        messages: [...currentThread.messages, newMsg]
      };

      setThreads(updatedThreads);
      localStorage.setItem(`elAtaba_chats_${userId}`, JSON.stringify(updatedThreads));
      setInputMessage('');
      
      // Simulate auto-reply after 2 seconds for a realistic presentation experience
      setTimeout(() => {
        const replyMsg: LocalMessage = {
          id: Math.random().toString(),
          senderId: currentThread.partyId,
          senderEmail: currentThread.partyEmail,
          recipientId: userId,
          recipientEmail: email || 'buyer@elataba.com',
          messageText: `Thanks for your interest! We received your inquiry: "${inputMessage.trim()}". A representative will review it shortly.`,
          sentAt: new Date().toISOString()
        };

        const replyThreads = [...updatedThreads];
        replyThreads[activeThreadIdx] = {
          ...replyThreads[activeThreadIdx],
          lastMessage: replyMsg.messageText,
          lastMessageTime: new Date().toISOString(),
          messages: [...replyThreads[activeThreadIdx].messages, replyMsg]
        };
        setThreads(replyThreads);
        localStorage.setItem(`elAtaba_chats_${userId}`, JSON.stringify(replyThreads));
      }, 2000);

    } catch (err) {
      console.error('Error sending message:', err);
      showToast('Error sending message. Simulated delivery anyway.', 'info');
      
      // Deliver locally even if backend is offline so the demo doesn't break
      const newMsg: LocalMessage = {
        id: Math.random().toString(),
        senderId: userId,
        senderEmail: email || 'buyer@elataba.com',
        recipientId: currentThread.partyId,
        recipientEmail: currentThread.partyEmail,
        messageText: inputMessage.trim(),
        sentAt: new Date().toISOString()
      };
      
      const updatedThreads = [...threads];
      updatedThreads[activeThreadIdx] = {
        ...currentThread,
        lastMessage: inputMessage.trim(),
        lastMessageTime: new Date().toISOString(),
        messages: [...currentThread.messages, newMsg]
      };
      setThreads(updatedThreads);
      localStorage.setItem(`elAtaba_chats_${userId}`, JSON.stringify(updatedThreads));
      setInputMessage('');
    } finally {
      setIsSending(false);
    }
  };

  const activeThread = activeThreadIdx !== null ? threads[activeThreadIdx] : null;

  const renderMessagesContent = () => (
    <div style={{ padding: '1rem' }}>
      <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--secondary)', marginBottom: '1.5rem' }}>Messages (الرسائل)</h1>

      <div className="card" style={{ padding: 0, display: 'flex', height: '650px', overflow: 'hidden', border: '1px solid var(--border-color)' }}>
        {/* Sidebar: Threads List */}
        <div style={{ width: '320px', borderRight: '1px solid var(--border-color)', display: 'flex', flexDirection: 'column', backgroundColor: 'var(--bg-main)' }}>
          <div style={{ padding: '1rem', borderBottom: '1px solid var(--border-color)', fontWeight: 'bold' }}>Conversations</div>
          <div style={{ flex: 1, overflowY: 'auto', display: 'flex', flexDirection: 'column' }}>
            {threads.map((t, idx) => (
              <button
                key={t.partyId}
                onClick={() => setActiveThreadIdx(idx)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.75rem',
                  padding: '1rem',
                  border: 'none',
                  borderBottom: '1px solid var(--border-color)',
                  borderLeft: activeThreadIdx === idx && language !== 'ar' ? '4px solid var(--primary)' : '4px solid transparent',
                  borderRight: activeThreadIdx === idx && language === 'ar' ? '4px solid var(--primary)' : '4px solid transparent',
                  backgroundColor: activeThreadIdx === idx ? 'rgba(255, 183, 3, 0.12)' : 'transparent',
                  cursor: 'pointer',
                  width: '100%',
                  textAlign: language === 'ar' ? 'right' : 'left',
                  color: 'var(--text-main)',
                  transition: 'background-color 0.2s'
                }}
              >
                <div style={{
                  padding: '0.5rem',
                  borderRadius: '50%',
                  backgroundColor: 'rgba(255, 183, 3, 0.15)',
                  color: 'var(--primary-hover)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  {t.isStore ? <Store size={18} /> : <User size={18} />}
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontWeight: 'bold', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{t.partyName}</div>
                  <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', marginTop: '0.2rem' }}>
                    {t.lastMessage}
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Chat Window */}
        {activeThread ? (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column', backgroundColor: 'var(--bg-card)' }}>
            {/* Chat Header */}
            <div style={{ 
              padding: '1rem 1.5rem', 
              borderBottom: '1px solid var(--border-color)', 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center',
              backgroundColor: 'rgba(255, 183, 3, 0.12)'
            }}>
              <div>
                <h3 style={{ fontSize: '1.1rem', margin: 0 }}>{activeThread.partyName}</h3>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{activeThread.partyEmail}</span>
              </div>
            </div>

            {/* Message History Area */}
            <div 
              ref={chatContainerRef}
              style={{ flex: 1, padding: '1.5rem', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '1rem' }}
            >
              {activeThread.messages.map((msg, index) => {
                const isMe = msg.senderId === userId;
                return (
                  <div
                    key={msg.id || index}
                    style={{
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: isMe ? 'flex-end' : 'flex-start',
                      width: '100%'
                    }}
                  >
                    {msg.productName && (
                      <div style={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: '0.3rem',
                        fontSize: '0.75rem',
                        backgroundColor: 'var(--bg-main)',
                        padding: '0.25rem 0.5rem',
                        borderRadius: 'var(--radius-sm)',
                        marginBottom: '0.25rem',
                        color: 'var(--text-muted)'
                      }}>
                        <Package size={12} />
                        <span>Regarding: <strong>{msg.productName}</strong></span>
                      </div>
                    )}
                    <div
                      style={{
                        padding: '0.75rem 1rem',
                        borderRadius: 'var(--radius-md)',
                        backgroundColor: isMe ? 'var(--primary)' : 'var(--bg-main)',
                        color: isMe ? '#023047' : 'var(--text-main)',
                        maxWidth: '70%',
                        wordBreak: 'break-word',
                        fontWeight: 500,
                        fontSize: '0.92rem',
                        boxShadow: 'var(--shadow-sm)'
                      }}
                    >
                      {msg.messageText}
                    </div>
                    <span style={{ fontSize: '0.7rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>
                      {new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </div>
                );
              })}
            </div>

            {/* Input Form Footer */}
            <form onSubmit={handleSendMessage} style={{ padding: '1rem', borderTop: '1px solid var(--border-color)', display: 'flex', gap: '0.75rem' }}>
              <input
                type="text"
                className="form-control"
                value={inputMessage}
                onChange={(e) => setInputMessage(e.target.value)}
                placeholder="Type your message here... اكتب رسالتك هنا"
                style={{ flex: 1 }}
                disabled={isSending}
                required
              />
              <button
                type="submit"
                className="btn btn-primary"
                disabled={isSending || !inputMessage.trim()}
                style={{ padding: '0.75rem 1.25rem' }}
              >
                <Send size={18} />
              </button>
            </form>
          </div>
        ) : (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', color: 'var(--text-muted)' }}>
            <MessageCircle size={64} style={{ marginBottom: '1rem' }} />
            <h3>No conversation selected</h3>
            <p>Select a thread from the side panel to view messages.</p>
          </div>
        )}
      </div>
    </div>
  );

  if (role === Role.Seller || role === Role.StoreManager) {
    return (
      <div className="dashboard-layout">
        <div style={{ width: '260px', flexShrink: 0, height: 'calc(100vh - 78px)', position: 'sticky', top: '78px' }}>
          <Sidebar type="seller" />
        </div>
        <div style={{ flex: 1, padding: '2rem', overflowX: 'hidden' }}>
          {renderMessagesContent()}
        </div>
      </div>
    );
  }

  if (role === Role.Admin) {
    return (
      <div className="dashboard-layout">
        <div style={{ width: '260px', flexShrink: 0, height: 'calc(100vh - 78px)', position: 'sticky', top: '78px' }}>
          <Sidebar type="admin" />
        </div>
        <div style={{ flex: 1, padding: '2rem', overflowX: 'hidden' }}>
          {renderMessagesContent()}
        </div>
      </div>
    );
  }

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      {renderMessagesContent()}
    </div>
  );
};

export default Messages;
