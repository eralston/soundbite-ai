import { useLayoutEffect, useState } from "react";

/**
 * React layout effect for tracking the width and height of the window dynamically
 * Keep in mind it's better to use vh and wh in CSS for layout and only use this wen you need to dynamic resize from React
 * @returns
 */
export const useWindowSize = (): number[] => {
  const [size, setSize] = useState([0, 0]);

  useLayoutEffect(() => {
    const updateSize = (): void => {
      console.log(
        `New window size: ${window.innerWidth}x${window.innerHeight}`
      );
      setSize([window.innerWidth, window.innerHeight]);
    };

    window.addEventListener("resize", updateSize);
    updateSize();

    return (): void => {
      window.removeEventListener("resize", updateSize);
    };
  }, []);

  return size;
};
