//import React, { useCallback, useRef, useContext, Context, useEffect } from 'react';
//import { VariableSizeList as List } from 'react-window';

//import { useWindowSize } from '../../modules/useWindowSize';

//// Chat message

//// Assuming the structure of 'message' is something like this:
//// Adjust it according to your actual message structure
//interface RowContent {
//  text: string;
//}

//interface RowProps {
//  message: RowContent;
//  index: number;
//}

//export const Row: React.FC<RowProps> = ({ index, children }) => {
//  const { setSize, windowWidth } = useContext(ChatContext);
//  const root = useRef<HTMLDivElement>(null);

//  useEffect(() => {
//    if (root.current) {
//      setSize(index, root.current.getBoundingClientRect().height);
//    }
//  }, [index, windowWidth, setSize]);

//  return (
//    <div ref={root} className="message">
//      {children}
//    </div>
//  );
//};

//// Chat History

//interface ChatHistoryProps {
//  listHeight: number;
//  chatHistoryRef: React.RefObject<HTMLDivElement>;
//  listRef: React.RefObject<List>;
//  chatHistory: Array<{ [key: string]: any }>; // Replace any with a more specific type if you have a defined shape for chat messages
//}

//interface ChatContextType {
//  setSize: (index: number, size: number) => void;
//  windowWidth: number;
//}

//export const ChatContext: Context<ChatContextType> = React.createContext<ChatContextType>({
//  setSize: () => { },
//  windowWidth: 0,
//});

//export const ChatHistory: React.FC<ChatHistoryProps> = ({ listHeight, chatHistoryRef, listRef, chatHistory }) => {
//  const sizeMap = useRef<{ [index: number]: number }>({});
//  const setSize = useCallback((index: number, size: number) => {
//    sizeMap.current = { ...sizeMap.current, [index]: size };
//  }, []);
//  const getSize = useCallback((index: number) => sizeMap.current[index] || 50, []);
//  const [windowWidth] = useWindowSize();

//  return (
//    <ChatContext.Provider value={{ setSize, windowWidth }}>
//      <div ref={chatHistoryRef} className="chatHistory">
//        {chatHistory.length > 0 && (
//          <List
//            height={listHeight}
//            itemCount={chatHistory.length}
//            itemSize={getSize}
//            width="100%"
//            ref={listRef}
//          >
//            {({ index, style }) => (
//              <div style={style}>
//                <Row index={index} message={chatHistory[index]} />
//              </div>
//            )}
//          </List>
//        )}
//      </div>
//    </ChatContext.Provider>
//  );
//};
