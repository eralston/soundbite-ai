import React from "react";

interface IProps {
  is?: boolean;
  children?: React.ReactNode;
  isDisplayBased?: boolean;
  className?: string;
}

/**
 * Shows its children only if the "is" property is true
 * @param props
 */
export const ShowWhen = (props: IProps) => {
  let show = false;
  if (props.is) show = true;

  let className = props.className || "";

  let display = "none";
  if (show) display = "block";

  return (
    <React.Fragment>
      {props.isDisplayBased ? (
        <div style={{ display: display, width: "100%" }} className={className}>
          {props.children}
        </div>
      ) : show ? (
        props.children
      ) : (
        <React.Fragment />
      )}
    </React.Fragment>
  );
};
