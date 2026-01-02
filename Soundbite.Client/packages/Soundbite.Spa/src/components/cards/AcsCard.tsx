import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  Button,
  Card,
  CardBody,
  CardFooter,
  CardHeader,
  Col,
  FormGroup,
  InputGroup,
  Row,
} from "reactstrap";

import { OrganizationsService, Utils } from "@soundbite/api";
import {
  ErrorDlg,
  Loader,
  ShowWhen,
  WidgetStore,
} from "@soundbite/widgets-react";

interface AcsCardProps {
  className?: string;
}

/** Configuration card for ACS custom notifications */
export const AcsCard: React.FC<AcsCardProps> = observer(
  (props: AcsCardProps) => {
    // Params
    let { orgRoute } = useParams<{ orgRoute: string }>();

    const [connString, setConnString] = useState<string | undefined>(undefined);
    const [fromEmail, setFromEmail] = useState<string | undefined>(undefined);
    const [fromName, setFromName] = useState<string | undefined>(undefined);
    const [isChanged, setIsChanged] = useState(false);
    const [isSaving, setIsSaving] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
      const load = async () => {
        try {
          setIsLoading(true);
          const initialSettings =
            await OrganizationsService.readAzureSettingsAsync(orgRoute);
          setConnString(initialSettings?.acsConnectionString);
          setFromName(initialSettings?.acsFromName);
          setFromEmail(initialSettings?.acsFromEmail);
          setIsChanged(false);
        } catch (err) {
          ErrorDlg.show(
            err,
            "Error Loading Azure Communication Services (ACS) Settings"
          );
        } finally {
          setIsLoading(false);
        }
      };
      load();
    }, [orgRoute]);

    const save = async (isResetting: boolean = false) => {
      try {
        setIsSaving(true);

        const settings = await OrganizationsService.readAzureSettingsAsync(
          orgRoute
        );
        if (settings == null) {
          throw new Error(
            "Error Reading Azure Communication Services (ACS) Settings Before Update"
          );
        }

        if (isResetting) {
          settings.acsConnectionString = undefined;
          settings.acsFromEmail = undefined;
          settings.acsFromName = undefined;
        } else {
          settings.acsConnectionString = connString;
          settings.acsFromEmail = fromEmail;
          settings.acsFromName = fromName;
        }

        await OrganizationsService.updateAzureSettingsAsync(orgRoute, settings);

        setIsChanged(false);
      } catch (err) {
        ErrorDlg.show(err, "Error Saving Notification Settings");
      } finally {
        setIsSaving(false);
      }
    };

    const onSave = async () => {
      await save(false);
    };

    const onReset = async () => {
      if (!window.confirm("Are You Sure You Want to Reset the Connection?")) {
        return;
      }
      await save(true);
      window.location.reload();
    };

    const hasValidEmail = () => {
      if (fromEmail == null || fromEmail === "") {
        return true;
      }

      return Utils.isEmail(fromEmail);
    };

    const isValid = (): boolean => {
      return (
        hasValidEmail() &&
        (connString?.length ?? 0) > 0 &&
        (fromEmail?.length ?? 0) > 0 &&
        (fromName?.length ?? 0) > 0
      );
    };

    return (
      <Card className={props.className}>
        <Loader
          isLoadedWhen={!!WidgetStore.organizations.currentOrg}
          style={{ minHeight: "12rem" }}
        >
          <CardHeader className="border-0">
            <Row className="align-items-center">
              <Col md="11">
                <h1 className="mb-0">
                  <i className="fas fa-envelope d-none d-sm-inline"></i>
                  {WidgetStore.organizations.currentOrg?.details.name} Custom
                  E-Mail Notifications Settings
                </h1>
                <div className="mt-0 text-muted">
                  Enabling custom senders for e-mail notifications from
                  Soundbite via{" "}
                  <a
                    href="https://azure.microsoft.com/en-us/products/communication-services/"
                    target="_blank"
                    rel="noreferrer"
                  >
                    Azure Communication Services
                  </a>
                </div>
              </Col>
              <Col md="1" className="text-right"></Col>
            </Row>
          </CardHeader>
          <Loader isLoadedWhen={!isLoading && !isSaving}>
            <CardBody>
              <FormGroup>
                <InputGroup>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Connection String"
                    aria-label="Connection String"
                    onChange={(e) => {
                      setConnString(e.target.value);
                      setIsChanged(true);
                    }}
                    title="Connection String"
                    value={connString}
                  />
                </InputGroup>
              </FormGroup>
              <ShowWhen is={!hasValidEmail()}>
                <div className="text-danger">
                  This is not a valid e-mail address
                </div>
              </ShowWhen>
              <FormGroup>
                <InputGroup>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="From E-Mail"
                    aria-label="From E-Mail"
                    onChange={(e) => {
                      setFromEmail(e.target.value);
                      setIsChanged(true);
                    }}
                    title="From E-Mail"
                    value={fromEmail}
                  />
                </InputGroup>
              </FormGroup>
              <FormGroup>
                <InputGroup>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="From Name"
                    aria-label="From Name"
                    onChange={(e) => {
                      setFromName(e.target.value);
                      setIsChanged(true);
                    }}
                    title="From Name"
                    value={fromName}
                  />
                </InputGroup>
              </FormGroup>
            </CardBody>
            <CardFooter className="d-flex justify-content-end">
              <ShowWhen
                is={!isChanged && connString != null && fromEmail != null}
              >
                <Button
                  color="danger"
                  className="btn-icon btn-3"
                  onClick={onReset}
                  title="Reset Custom E-Mail Settings"
                >
                  <span className="btn-inner--text">Reset</span>
                </Button>
              </ShowWhen>
              <Button
                color="primary"
                className="btn-icon btn-3"
                onClick={onSave}
                disabled={!isChanged || isSaving || !isValid()}
                title="Save Custom E-Mail Settings"
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
        </Loader>
      </Card>
    );
  }
);
