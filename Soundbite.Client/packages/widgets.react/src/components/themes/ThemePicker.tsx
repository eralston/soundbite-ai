/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { Component } from "react";
import { Button, Card, CardFooter, CardHeader } from "reactstrap";

import { OrganizationsService, ThemeName } from "@soundbite/api";

import { ShowWhen, Toggle } from "../controls";
import { lightTheme, GlobalTheme } from "../../styles";
import { WidgetStore } from "../../store";
import { ThemeConfig } from "./ThemeConfig";
import { DialogManager, DialogStyleType } from "../../dialogs";

const baseStyles = css`
  .sb-theme-toggle {
    display: flex;
    align-items: center;
    padding: 0.5rem;
    background-color: ${GlobalTheme.current.colors.neutrals.midground};
  }

  .sb-theme-toggle label {
    margin: 0 0.5rem;
  }
`;

interface IProps {}

interface IState {
  isEditing: boolean;
}

export class ThemePicker extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    this.onCancel = this.onCancel.bind(this);
    this.onEdit = this.onEdit.bind(this);
    this.state = {
      isEditing: GlobalTheme.current.name === ThemeName.Draft,
    };
  }

  onEdit() {
    this.setState({ isEditing: true });
  }

  onPreview() {
    // TODO: Replace with currentOrgTheme ?? lightTheme logic
    const lightThemeJson = JSON.stringify(lightTheme);
    const currentThemeJson = JSON.stringify(GlobalTheme.current);
    if (lightThemeJson !== currentThemeJson) {
      GlobalTheme.current.name = ThemeName.Draft;
      const orgRoute = WidgetStore.organizations.currentOrg?.details.route;
      if (orgRoute == null) {
        return;
      }
      GlobalTheme.set(GlobalTheme.current, orgRoute, true);
      GlobalTheme.apply();
    }
  }

  async onCancel(): Promise<void> {
    this.setState({ isEditing: false });
  }

  onReset() {
    DialogManager.ShowConfirmDialog(
      "Reset Theme",
      "Are you sure you want to reset the theme for your organization? All users will see the reset them after their next full refresh",
      async () => {
        const orgRoute = WidgetStore.organizations.currentOrg?.details.route;
        if (orgRoute == null) {
          return;
        }
        await OrganizationsService.updateThemeAsync(orgRoute, undefined);
        GlobalTheme.clear(orgRoute);
        GlobalTheme.apply();
      },
      "Reset",
      DialogStyleType.Danger
    );
  }

  render() {
    const isDraft = GlobalTheme.current.name === ThemeName.Draft;
    return (
      <Card css={baseStyles} className="mb-2">
        <CardHeader>
          <h1 className="mb-0">{`${WidgetStore.organizations.currentOrg?.details.name} Theme: ${GlobalTheme.current.name}`}</h1>
          <p className="mt-0 text-muted">
            Customize the visual styling of your organization on Soundbite
          </p>
          <ShowWhen is={this.state.isEditing && !isDraft}>
            <ThemeConfig />
          </ShowWhen>
        </CardHeader>
        <ShowWhen is={!isDraft}>
          <CardFooter className="text-right">
            <ShowWhen is={!this.state.isEditing}>
              <Button color="primary" onClick={this.onEdit}>
                Edit
              </Button>
              <ShowWhen is={GlobalTheme.current.name === ThemeName.Custom}>
                <Button
                  color="secondary"
                  className="sb-link-simple-btn"
                  onClick={this.onReset}
                >
                  Reset
                </Button>
              </ShowWhen>
            </ShowWhen>
            <ShowWhen is={this.state.isEditing}>
              <Button color="secondary" outline={true} onClick={this.onCancel}>
                Cancel
              </Button>
            </ShowWhen>
          </CardFooter>
        </ShowWhen>
      </Card>
    );
  }
}
