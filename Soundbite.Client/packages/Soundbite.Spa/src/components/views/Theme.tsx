import React from "react";
import { observer } from "mobx-react-lite";
import { ThemeDemo, ThemePicker } from "@soundbite/widgets-react";

import { Private } from "./Private";

const Theme: React.FC = observer(() => {
  return (
    <Private>
      <div className="mt-7">
        <ThemePicker />
        <ThemeDemo />
      </div>
    </Private>
  );
});

export default Theme;
