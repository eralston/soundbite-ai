/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React from "react";
import { Button, Card, CardBody, CardFooter } from "reactstrap";

import { ThemeName } from "@soundbite/api";

import { GlobalTheme, lightTheme } from "../../styles";
import { ColorConfig } from "./ColorConfig";
import { ShowWhen } from "../controls";
import { WidgetStore } from "../../store/WidgetStore";

const baseStyles = css`
  position: fixed;
  bottom: 0;
  left: 0;
  margin-right: auto;
  border-top: 1px solid ${GlobalTheme.current.colors.neutrals.max};
  border-right: 1px solid ${GlobalTheme.current.colors.neutrals.max};
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 0;
  border-top-left-radius: 0;
  max-height: 90%;
  max-width: 25rem;
  z-index: 2;

  .sb-theme-toggle label {
    margin: 0 0.5rem;
  }
`;

/** A card fixed in the view of the browser that enables changing theme elements */
export const ThemeConfig: React.FC = () => {
  const orgRoute = WidgetStore.organizations.currentOrg?.details.route;

  function onPreview() {
    if (orgRoute != null) {
      GlobalTheme.current.name = ThemeName.Draft;
      GlobalTheme.setDraft(GlobalTheme.current, orgRoute);
      GlobalTheme.apply();
    }
  }

  return (
    <Card className="sb-theme-config" css={baseStyles}>
      <CardBody>
        <h1 className="text-sm text-uppercase">Configure Theme</h1>
        <ColorConfig />
      </CardBody>
      <ShowWhen is={orgRoute != null}>
        <CardFooter>
          <div className="text-right">
            <Button color="primary" onClick={onPreview}>
              Preview
            </Button>
          </div>
        </CardFooter>
      </ShowWhen>
    </Card>
  );
};
