import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardFooter, CardHeader, Col, Row } from "reactstrap";

import { OrganizationsService, Utils } from "@soundbite/api";
import { Loader, WidgetStore } from "@soundbite/widgets-react";

import { TextRow, ToggleTable } from "../controls/ToggleTable";
import { Private } from "./Private";

export const OrgTokenSettings: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  const [isChanged, setIsChanged] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isGenerated, setIsGenerated] = useState(false);
  const [isEncoded, setIsEncoded] = useState(false);
  const [token, setToken] = useState("");

  useEffect(() => {}, [orgRoute]);

  async function onSave(): Promise<void> {
    setIsSaving(true);
    try {
      await OrganizationsService.setOrgToken(orgRoute, token);
      setIsChanged(false);
      setIsGenerated(false);
    } finally {
      setIsSaving(false);
    }
  }

  async function onSyncTokenWithAms(): Promise<void> {
    setIsSaving(true);
    try {
      await OrganizationsService.syncOrgTokenWithAmsContentKeyPolicy(orgRoute);
      alert("Succeeded");
    } catch (ex) {
      alert("Failed");
    } finally {
      setIsSaving(false);
    }
  }

  function onEncode(): void {
    setToken(btoa(token));
    setIsEncoded(true);
  }

  function onGenerate(): void {
    const length = 28;
    const characters =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
    const charactersLength = characters.length;

    let result = "";
    let counter = 0;

    while (counter < length) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
      counter += 1;
    }

    setIsEncoded(false);
    setIsGenerated(true);
    setToken(btoa(result));
    setIsChanged(true);
  }

  return (
    <Private>
      <Card>
        <Loader
          isLoadedWhen={!!WidgetStore.organizations.currentOrg}
          style={{ minHeight: "12rem" }}
        >
          <CardHeader className="border-0">
            <Row className="align-items-center">
              <Col md="11">
                <h1 className="mb-0">
                  <i className="fas fa-bell d-none d-sm-inline"></i>
                  {WidgetStore.organizations.currentOrg?.details.name} Security
                  Token
                </h1>
                <div className="mt-0 text-muted">
                  Configure the security token for your organization
                </div>
              </Col>
              <Col md="1" className="text-right"></Col>
            </Row>
          </CardHeader>
          <div className="table-responsive">
            <ToggleTable hideHeader={true}>
              <tr>
                <td colSpan={2} style={{ whiteSpace: "normal" }}>
                  <div>
                    <b>Warning:</b> changing the token invalidates all current
                    user tokens and updates Azure Media Services AES clear key
                    encryption settings associated with the Content Policy for
                    the organization. Azure may take an unspecified amount of
                    time to apply the token changes which could make video
                    inaccessible for a time. Do not change this setting without
                    understanding the ramifications of doing so.
                  </div>
                </td>
              </tr>
              <TextRow
                title="Token Key"
                subtitle="Base 64 Encoded Security Token Key"
                onChange={(value) => {
                  if (Utils.isNullOrEmpty(value)) {
                    setIsGenerated(false);
                    setIsEncoded(false);
                  }
                  setToken(value);
                  setIsChanged(true);
                }}
                defaultValue=""
                value={token}
              />
            </ToggleTable>
          </div>
          <CardFooter className="d-flex justify-content-end">
            <Button
              color="primary"
              className="btn-icon btn-3"
              onClick={onSyncTokenWithAms}
              title="Sync Org Token with AMS"
            >
              <span className="btn-inner--icon">
                <i className="fas fa-arrow-up"></i>
              </span>
              <span className="btn-inner--text">
                <Fragment>&nbsp;</Fragment>Sync Org Token with AMS
              </span>
            </Button>
            <Button
              color="primary"
              className="btn-icon btn-3"
              onClick={onEncode}
              disabled={
                !isChanged ||
                Utils.isNullOrEmpty(token) ||
                isGenerated ||
                isEncoded
              }
              title="Encode Current Value"
            >
              <span className="btn-inner--icon">
                <i className="fas fa-arrow-right"></i>
              </span>
              <span className="btn-inner--text">
                <Fragment>&nbsp;</Fragment>Encode Current Value
              </span>
            </Button>
            <Button
              color="primary"
              className="btn-icon btn-3"
              onClick={onGenerate}
              title="Generate a Random Token"
            >
              <span className="btn-inner--icon">
                <i className="fas fa-key"></i>
              </span>
              <span className="btn-inner--text">
                <Fragment>&nbsp;</Fragment>Generate Token
              </span>
            </Button>
            <Button
              color="primary"
              className="btn-icon btn-3"
              onClick={onSave}
              disabled={!isChanged && !isSaving}
              title="Save Notification Settings"
            >
              <span className="btn-inner--icon">
                <i className="fas fa-save"></i>
              </span>
              <span className="btn-inner--text">
                <Fragment>&nbsp;</Fragment>Save
              </span>
            </Button>
          </CardFooter>
        </Loader>
      </Card>
    </Private>
  );
});
