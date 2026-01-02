/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { useState } from "react";
import { Button, Col, Row } from "reactstrap";

import { OrganizationsService, ThemeName } from "@soundbite/api";

import { GlobalTheme, ThemeAssert } from "../../styles";
import { ShowWhen } from "../controls/ShowWhen";
import { ThemeConfig } from "./ThemeConfig";
import { DialogManager, DialogStyleType } from "../../dialogs";
import { WidgetStore } from "../../store/WidgetStore";

const styles = css`
  background-color: ${GlobalTheme.current.colors.neutrals.midground};
  border-bottom: 3px solid ${GlobalTheme.current.colors.neutrals.background};
  color: ${GlobalTheme.current.colors.neutrals.max};
  z-index: 3;
  padding: 1rem;

  @media (min-width: 768px) {
    margin-left: 250px;
  }
`;

/** A bar for the top of the page used to indicate theme configuration status and actions */
export const ThemeBanner: React.FC = () => {
  const [isEditing, setIsEditing] = useState(false);

  function onEdit(): void {
    setIsEditing(!isEditing);
  }

  async function onPublish(): Promise<void> {
    DialogManager.ShowConfirmDialog(
      "Publish Theme",
      "Are you sure you want to publish this theme? The next time users fully refresh Soundbite, they will see your new theme",
      async () => {
        const orgRoute = WidgetStore.organizations.currentOrg?.details.route;
        if (orgRoute == null) {
          return;
        }
        GlobalTheme.current.name = ThemeName.Custom;
        const newTheme = await OrganizationsService.updateThemeAsync(
          orgRoute,
          GlobalTheme.current
        );
        GlobalTheme.set(newTheme, orgRoute, true);
        GlobalTheme.apply();
      }
    );
  }

  async function onAbandon(): Promise<void> {
    DialogManager.ShowConfirmDialog(
      "Abandon Draft",
      "Are you sure you want to abandon your draft theme?",
      () => {
        const orgTheme = WidgetStore.organizations.currentOrg?.settings?.theme;
        if (orgTheme != null) {
          const orgRoute = WidgetStore.organizations.currentOrg?.details.route;
          if (orgRoute != null) {
            GlobalTheme.set(orgTheme, orgRoute, true);
          }
        } else {
          GlobalTheme.clear(
            WidgetStore.organizations.currentOrg?.details.route
          );
        }
        GlobalTheme.apply();
      },
      "Abandon",
      DialogStyleType.Danger
    );
  }

  const asserts = (GlobalTheme.lastTester?.asserts ?? []).filter(
    (assert) => assert.message != null
  );
  return (
    <React.Fragment>
      <ShowWhen is={GlobalTheme.isNeedReload}>
        <div className="" css={styles}>
          <Row>
            <Col>New Theme is Available; Reload to Apply</Col>
            <Col className="text-right">
              <Button
                className="btn-sm"
                color="primary"
                onClick={() => {
                  GlobalTheme.apply();
                }}
              >
                Reload
              </Button>
            </Col>
          </Row>
        </div>
      </ShowWhen>
      <ShowWhen is={GlobalTheme.current.name === ThemeName.Draft}>
        <div className="" css={styles}>
          <h1 className="text-muted text-sm uppercase">DRAFT THEME</h1>
          <p className="text-muted">
            You are previewing this draft theme. Until you publish your changes,
            no one else will see these changes.
          </p>
          <ShowWhen is={asserts.length > 0}>
            <div className="bg-warning p-2 rounded mb-2">
              <h2 className="text-white text-sm text-uppercase">
                Theme Warnings
              </h2>
              <ul>
                {asserts.map((assert: ThemeAssert) => {
                  return <li>{assert.message}</li>;
                })}
              </ul>
            </div>
          </ShowWhen>
          <ShowWhen is={isEditing}>
            <ThemeConfig />
          </ShowWhen>
          <div className="text-right">
            <Button color="secondary" onClick={onPublish}>
              Publish
            </Button>
            <Button color="secondary" onClick={onEdit} outline={!isEditing}>
              Edit
            </Button>
            <Button color="secondary" outline={true} onClick={onAbandon}>
              Abandon
            </Button>
          </div>
        </div>
      </ShowWhen>
    </React.Fragment>
  );
};
