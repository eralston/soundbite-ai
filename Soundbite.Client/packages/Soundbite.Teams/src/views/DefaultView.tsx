import React from "react";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  children?: React.ReactNode;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const DefaultView: React.FC<IProps> = (props: IProps) => {
  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <div style={{ textAlign: "center" }}>
      <img src="/img/default.png" style={{ width: "100%" }} alt="profile" />
      <div
        style={{
          position: "absolute",
          left: 0,
          top: 0,
          backgroundColor: "#421B50",
          width: "100%",
          height: "100%",
          zIndex: -100,
        }}
      ></div>
    </div>
  );
};
